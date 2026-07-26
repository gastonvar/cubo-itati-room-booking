type UnauthorizedHandler = () => void

function defaultUnauthorizedRedirect(): void {
  const loginPath = `${import.meta.env.BASE_URL}login`.replace(/\/+/g, '/')
  if (!window.location.pathname.endsWith('/login')) {
    window.location.assign(loginPath.startsWith('/') ? loginPath : `/${loginPath}`)
  }
}

let onUnauthorized: UnauthorizedHandler = defaultUnauthorizedRedirect

/** Wired by AuthProvider so Axios interceptors can clear session without hooks. */
export function bindUnauthorizedHandler(handler: UnauthorizedHandler): () => void {
  onUnauthorized = handler
  return () => {
    onUnauthorized = defaultUnauthorizedRedirect
  }
}

export function notifyUnauthorized(): void {
  onUnauthorized()
}
