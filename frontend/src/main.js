// src/main.js

import './assets/main.css'
import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import App from './App.vue'
import router from './router'

import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'

// Suppress the Vite HMR “message channel closed” warning
window.addEventListener('unhandledrejection', event => {
  const msg = event.reason?.message || ''
  if (msg.includes('message channel closed before a response')) {
    event.preventDefault()
  }
})

const app = createApp(App)

// Build and start the SignalR connection (proxied by Vite)
const connection = new HubConnectionBuilder()
  .withUrl('/gameHub', { withCredentials: true })
  .configureLogging(LogLevel.Information)
  .withAutomaticReconnect()
  .build()

// connection.on('PotentiometerUpdated', data => {
//   console.log('PotentiometerUpdated:', data)
// })

connection.on('IterationStarted', iteration => {
  console.log('IterationStarted:', iteration)
})

async function startSignalR() {
  try {
    await connection.start()
    console.log('SignalR connected to /gameHub')
  } catch (err) {
    console.error('SignalR connection error:', err)
    setTimeout(startSignalR, 4000)
  }
}

startSignalR()

// Expose the connection globally if needed
app.config.globalProperties.$signalR = connection

// Standard Vue plugin setup
app.use(createPinia())
app.use(router)
app.use(ElementPlus)
app.mount('#app')