# Multimodal Arduino Memory Game

A full-stack, multimodal interactive cognitive training platform that bridges physical hardware (Arduino) with a modern web interface (Vue.js) and a robust backend (.NET Core) to provide engaging memory and logic mini-games.

---

## Features

* **Multimodal Interaction**: Combines visual (LED patterns), auditory, textual cues, and physical inputs (servo motors, potentiometers).
* **Progressive Difficulty**: Features multiple memory mini-games where sequences and challenges get progressively harder as players succeed.
* **Real-time Synchronization**: Utilizes **SignalR** for low-latency, real-time event broadcasting between the hardware, backend, and web clients.
* **Hardware-Software Bridge**: Seamlessly translates physical sensor inputs and hardware states into web actions via **Serial Communication**.

---

## Tech Stack

* **Frontend**: Vue.js
* **Backend**: C#, ASP.NET Core, SignalR
* **Hardware**: Arduino (C++), Serial Communication 

---

## System Architecture & Workflow

1. **Client Actions (REST API)**: When a user interacts with the web interface (e.g., clicking **Start**, **Abort**, or **Submit Answer**), a standard RESTful HTTP request is sent to the ASP.NET Core backend.
2. **Hardware & Serial IO (Arduino)**: The backend processes the request and communicates with the Arduino hardware via USB serial streams (`SerialPort`). The Arduino handles physical sensors, LEDs, and buzzers.
3. **Real-time Feedback (SignalR)**: Game results, state changes, and hardware events are pushed back to the web frontend in real-time via SignalR hubs, ensuring instant visual and auditory feedback.
