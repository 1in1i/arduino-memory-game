import { createRouter, createWebHistory } from 'vue-router'
import HomePage from '../components/HomePage.vue'
import RotationGame from '@/components/RotationGame.vue'
import ReplicateNumber from '@/components/ReplicateNumber.vue'
import ReplicateColor from '@/components/ReplicateColor.vue'
import CountNotes from '@/components/CountNotes.vue'  
import EchoSounds from '@/components/EchoSounds.vue'
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'Home',
      component: HomePage,
    },
    {
      path: '/Rotation-Game',
      name: 'RotationGame',
      component: RotationGame
    },
    {
      path: '/Replicate-Number',
      name: 'ReplicateNumber',
      component: ReplicateNumber,
    },
     {
      path: '/Replicate-Color',
      name: 'ReplicateColor',
      component: ReplicateColor,
    },
    {
      path: '/Count-Notes',
      name: 'CountNotes',
      component: CountNotes,
    },
    {
      path: '/Echo-Sounds',
      name: 'EchoSounds',
      component: EchoSounds,
    }
  ],
})

export default router
