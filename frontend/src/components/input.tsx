import { type InputHTMLAttributes, forwardRef } from 'react'

type InputProps = InputHTMLAttributes<HTMLInputElement> & {
  label?: string
  error?: string
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, className = '', id, ...props }, ref) => {
    const inputId = id ?? props.name

    return (
      <div className="space-y-1.5">
        {label ? (
          <label
            htmlFor={inputId}
            className="block text-sm font-medium text-charcoal/80"
          >
            {label}
          </label>
        ) : null}
        <input
          ref={ref}
          id={inputId}
          className={`w-full rounded-xl border border-stone-300/80 bg-white/70 px-4 py-2.5 text-charcoal shadow-sm transition-colors placeholder:text-stone-400 focus:border-teal-accent focus:bg-white focus:outline-none focus:ring-2 focus:ring-teal-accent/20 ${error ? 'border-red-400 focus:border-red-400 focus:ring-red-200' : ''} ${className}`}
          {...props}
        />
        {error ? <p className="text-sm text-red-600">{error}</p> : null}
      </div>
    )
  },
)

Input.displayName = 'Input'
