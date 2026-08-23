<template>
  <div class="home-page">
    <h1 class="title">Welcome to Memory Game! </h1>
    <div class="game-buttons">
      <button class="game-button" 
          v-for="(game,index) in games" 
          :key="game.name"   
          :style="{ background: buttonColors[index].bg}" 
          @click="goToGame(game.route)">
        <span class="game-name">{{ game.name }}</span>
      </button>
    </div>
    <div class="video-wrapper">
        <video controls max-width="700px" height="400">
            <source src="../../public/video/meme.mp4" />
            Your browser doesn't support playing this video.
        </video>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      games: [
        { name: 'Copy Rotations', route: "RotationGame", maxStars: 3 },
        { name: 'Replicate Numbers', route:"ReplicateNumber", maxStars: 3 },
        { name: 'Replicate Colors', route:"ReplicateColor", maxStars: 4 },
        { name: 'Count Notes', route:"CountNotes", maxStars: 3 },
        { name: 'Echo Sounds', route:"EchoSounds", maxStars: 3 }
      ],
      buttonColors: [
        { bg: '#FFB6B9'},
        { bg: '#FFDAC1' },
        { bg: '#E2F0CB' },
        { bg: '#B5EAD7' },
        { bg: '#C7CEEA' } 
      ],
    }
  },
  methods:{
    goToGame(route){
      this.$router.push({name: route})
    },
  }

}
</script>

<style scoped>
.home-page {
  padding: 2rem;
  width: 100%;
  /* max-width: 800px; */
  margin: 0 auto;

}

.title {
  text-align: center;
  margin-bottom: 2.5rem;
  font-family: 'Comic Sans MS', 'Baloo 2', cursive;
  color: #2c3e50;
  font-size: 3rem;
  letter-spacing: 1.5px;
  text-shadow: 2px 2px 4px rgba(0,0,0,0.1);
}

.game-buttons {
  /* display:flex; */
  display: grid;
  grid-auto-flow: column; 
  grid-auto-columns: minmax(180px, 1fr); 
  overflow-x: auto;
  gap: 1.5rem;
}

.game-button {
  position: relative;
  height: 120px;
  width: 100%;
  border: none;
  border-radius: 12px;
  /* background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%); */
  box-shadow: 
    0 4px 6px rgba(0, 0, 0, 0.1),
    inset 0 1px 0 rgba(255, 255, 255, 0.3);
  cursor: pointer;
  transition: all 0.3s ease;
  overflow: hidden;
  padding: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.game-button:hover {
  transform: translateY(-5px);
  box-shadow: 
    0 7px 14px rgba(0, 0, 0, 0.15),
    inset 0 1px 0 rgba(255, 255, 255, 0.3);
    filter: brightness(1.03);

}
.game-button:active {
  transform: translateY(-2px);
}
.game-name {
  font-size: 1rem;
  font-weight: 600;
  color: #34495e;
  z-index: 2;
}

.star-badge {
  position: absolute;
  left: 12px;
  bottom: 12px;
  display: flex;
  gap: 3px;
}

.star {
  font-size: 1.2rem;
  color: #d0d0d0; 
}

.star.filled {
  color: #FFD700; 
  text-shadow: 0 0 4px rgba(255, 215, 0, 0.5);
}

.game-button:hover::after {
  opacity: 1;
  top: -30%;
  left: -30%;
}

.video-wrapper {
  display: flex;
  justify-content: center;
}

video {
  border-radius: 15px;
  box-shadow: 0 10px 20px rgba(0, 0, 0, 0.2);
  margin-top: 30px;
}

</style>