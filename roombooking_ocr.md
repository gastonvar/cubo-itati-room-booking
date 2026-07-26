TECHNICAL CHALLENGE

© Promtior

Technical challenge instructions

Introduction
At Promtior, we specialize in implementing cutting-edge generative solutions, with
a focus on the <implementation of agentic architecture to meet the diverse needs

of our clients.

Main goal of the Challenge
Build a chatbot with tool-calling capabilities that allows users to book a meeting

room through a conversational interface.

# Page 6

© Promtior TECHNICAL CHALLENGE INSTRUCTIONS

Functionality

The chatbot's sole purpose is to manage meeting room bookings, like reservation

systems you can find in large corporate offices.

Overview:

The office is located in Cubo Itati and has five rooms: A, B, C, D, and E.
Bookings are made in 30-minute slots.

Each room has a maximum capacity. When creating a booking, the user
must specify the number of attendees, which cannot exceed that limit.
A slot can only be held by one booking at a time — no double bookings.
Consecutive slots can be combined into a single appointment, up to a
maximum duration of 3 hours.

The system has two users — Userl and User2 — both authenticated with
the password TechnicalChallengePromtior.

Appointments in the same room must not overlap. For example, if
Appointment I runs from 10:00 to 11:30, Appointment 2 cannot start
before 11:30, as any earlier start time would conflict with the 10:00-1130
slot.

Every appointment requires a title (eg,, “Interview with John Doe’).



# Page 7

© Promtior

Technical requirements

: Implement a room-booking system that matches the rules defined in
the Introduction section (rooms A-E, room-specific capacities, 30-minute
slots, max. 3-hour bookings, no overlaps).

: Implement user authentication (login) for Userl and User2 using the
provided password

. Build a chatbot/assistant that interacts with the booking system via tool
calling. The chatbot must support at least the following actions:

© Create a booking for a given room, date/time range, title,
and number of attendees, and associate it with the currently
logged-in user.

© _ List available rooms for a requested time range

© Retrieve the schedule for a specific room (available vs. occupied

slots) for a requested date/time range.



# Page 8

© Promtior TECHNICAL REQUIREMENTS

o Cancel booking made by the currently logged-in user.
° Enforce booking constraints and validation:
© Only contiguous 30-minute slots may be combined into a single

booking, up to a maximum duration of 3 hours.
: In addition to implementing the system, you must submit a Jupyter
notebook that explains the technologies used and includes

code examples of these technologies applied to your solution



# Page 9

© Promtior

Required documentation

Objective of the Documentation
Provide a comprehensive overview of the approach, implementation, and technologies used
to develop the chatbot assistant, enabling a detailed evaluation of the proposed solution.

Include comments on the work carried out to reach the final solution, describing the step-by-

step process followed, the key decisions made, and the main challenges encountered during ,
development. 

Required Content

Project Overview: A brief summary in your own words about how you approached and ye Teerenareny
solved the challenge. This includes the implementation logic, the main challenges TTT
encountered, and how you overcame them. Pi a
We
Component Diagram: A diagram showing the components involved in the solution and Wee 9,
their interactions from the time the question is received by the chatbot until the ‘ %, “9
response is given "Heeg,
Sita,
‘© Tip: You can use the following tools: Draw.io, Lucidchart, Excalidraw.



# Page 10

© Promtior

Technologies and stack

Technologies to be used
Implement the solution using the stack of your choice. Use the OpenAl API if you have a
subscription. Otherwise, for local development, you can use Ollama and LLaMA as explained

in the LangChain documentation. For cloud deployment without a paid subscription, use : eee
op peemerent

Grog — a free API, no credit card required, compatible with LangChain via langchain-groq, " evoowecerieel 9 OumenOOnetl tase
supporting models like Llama 3 and Mixtral. OpenRouter can also be used as an alternative haan

Lea ET TT TITY Rete

provider to access different models through a unified API. 




# Page 11

© Promtior TECHNOLOGIES AND STACK

Deployment
Deploy the solution on a cloud of your choice (AWS, Azure, GCP)

Tip: You can also use Railway to simplify deployment.

& Note: Deploying Ollama in the cloud requires significant RAM (4—12 GB depending on

the model) and is not viable ona free tier. 



# Page 12

© Promtior

Delivery format and
considerations

Source Code and Documentation
The source code and documentation must be uploaded to a public GitHub repository.

The required documentation should be uploaded in the same repository within the /doc
folder.

Once the source code is uploaded, the following form must be completed
https://forms.microsoft.com/r/2Q6iZpYvPT

Considerations
If you have any questions about the challenge instructions or the delivery format, you can

ask us at challenges@ promtior.ai and we will respond as soon as possible.



# Page 13

© Promtior

Final words

This challenge will help us better understand your current knowledge and seniority.

Itis designed so that someone with programming knowledge and good comprehension
skills can solve it without problems by relying on the shared documentation.

We wish you great success in this process, and we hope to continue the transition to a
Bionic future together!!!



# Page 14

©& challenges@promtior.ai

@ Promtior

Contact

