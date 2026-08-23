<script setup>
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <div id="app">
    <RouterView />
  </div>
</template>
<style>
html, body{
  height: 100%;
  margin: 0;
  padding: 0;
  /* background: linear-gradient(to bottom, #fdfbfb, #ebedee); */
background: radial-gradient(circle,rgba(255, 248, 251, 1) 0%, rgba(255, 228, 240, 1) 50%);}
#app{
  height: 100% ;
  width: 100%;
  display: flex;
  flex-direction: column;
  /* overflow: auto; */
  position: relative;
  /* background: linear-gradient(to bottom, #fdfbfb, #ebedee); */
/* background: radial-gradient(circle,rgba(255, 248, 251, 1) 40%, rgba(255, 228, 240, 1) 70%); */
}
</style>
