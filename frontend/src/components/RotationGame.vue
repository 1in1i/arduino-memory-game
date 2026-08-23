<template>
  <div class="rotation-game">
      <h1 class="title" >Copy Rotations</h1>
       <div class="lives">
         <img v-for="(life, index) in totalLives" 
         :key="index"
         :src="index < (3 - errorCount) ? heartIcon : brokenheartIcon"
         class="heart">
        </div>
      <div class="sidebar">
        <div class="instructions">
          <h2 class="instruction" style="font-weight: bold;">Instructions</h2>
          <p class="text" data-icon="👉">Click the Start button to begin.</p>
          <p class="text" data-icon="👀">Watch and memorize the angle sequence shown in the game.</p>
          <p class="text" data-icon="🧠">Replicate the angle sequence.</p>
          <p class="text" data-icon="🌟">Each time you get it right, the game gets harder, the sequence gets longer.</p>
          <p class="text" data-icon="❤️">You have 3 hearts (lives):</p>
          <p class="text" data-icon="">If you make a mistake, one heart will go. When all 3 hearts are gone, the game ends.</p>
          <p class="text" data-icon="🛑">You can also press the Abort button to stop the game whenever you like.</p>
        </div>
        <div class="records">
          <h2 class="record" style="font-weight: bold;">Records</h2>
          <p>Mistakes: {{ errorCount }}</p>
          <p>Time: {{ elapsedSeconds === null ? '00:00' : formattedTime }}</p>
        </div>
      </div>

      <div class="buttons">
        <el-button class="button"
        :type="isRunning ? 'danger' : 'success'"
        round
        @click="isRunning? abortGame(this.gameId) : startGame(this.gameId)"        
      >
        {{ isRunning ? 'Abort' : 'Start' }}
      </el-button>
      </div>
  </div>
</template>
<script>
export default {
  data() {
    return {
      isRunning: false,
      totalLives : 3,
      heartIcon : '../public/icon/heart1.png',
      brokenheartIcon: '../public/icon/heart2.png',
      gameName: 'Copy Rotation',
      connection: null,
      errorCount: 0,
      elapsedSeconds: 0,
      timer: null,
      gameId: 1,
      sequenceHandler: null,
      hasGameOver: false

    }
  },
  computed: {
    formattedTime() {
      const mins = Math.floor(this.elapsedSeconds / 60)
      const secs = this.elapsedSeconds % 60
      return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`
    }
  },
  methods: {
     saveGameState(gameName, elapsedSeconds) {
      const state = { elapsedSeconds };
      sessionStorage.setItem(`gameState_${gameName}`, JSON.stringify(state));
    },
    loadGameState(gameName) {
      const stateStr = sessionStorage.getItem(`gameState_${gameName}`);
      if (stateStr) {
        return JSON.parse(stateStr);
      }
      return null;
    },
   async startGame(gameId){
    this.elapsedSeconds = null;
    this.isRunning = true;
    this.errorCount = 0;
    this.timer = setInterval(() => {
        this.elapsedSeconds++
      }, 1000)
     try {
        const response = await fetch(`/api/game/game/start/${gameId}`, {
            method: "POST"
        });

        if (response.ok) {
            console.log("Game started successfully.");
        } else {
            console.error("Failed to start game.");
        }
    } catch (err) {
        console.error("Error starting game:", err);
    }
   },
   async abortGame(gameId){
    this.isRunning = false;
     if (this.timer) {
        clearInterval(this.timer)
        this.timer = null
      };
    try {
        const response = await fetch(`/api/game/${gameId}/quit`, {
            method: "POST"
        });
        if (response.ok) {
            console.log("Game aborted successfully.");
        } else {
            console.error("Failed to abort game.");
        }
    } catch (err) {
        console.error("Error aborting game:", err);
    }
    setTimeout(() => {
    this.$router.push('/');
  }, 100);
   },
  handleSequenceResult(result){
    if(result == false){
          this.errorCount++
    }
    if(this.errorCount >= 3 && !this.hasGameOver){
      this.hasGameOver = true
      this.isRunning = false;
      this.$alert('GAME OVER!', {
      confirmButtonText: 'OK'
    }).then(()=>{
          this.abortGame();
          // this.$router.push('/');
    });      
    return;
    }
   },
  },
   async mounted() {
    const savedState = this.loadGameState(this.gameName);
    if (savedState) {
      this.elapsedSeconds = savedState.elapsedSeconds;
    }
    const connection = this.$signalR;
    connection.off("sequenceResult", this.sequenceHandler);
     this.sequenceHandler = this.handleSequenceResult;
    connection.on("sequenceResult",this.sequenceHandler);
  },
  beforeDestroy() {
  const connection = this.$signalR;
  connection.off("sequenceResult", this.sequenceHandler);
},
  watch: {
    elapsedSeconds(newVal) {
      this.saveGameState(this.gameName, newVal);
    },
  },
  
}
</script>
<style scoped>
.rotation-game {
  display: grid;
  background: radial-gradient(circle,rgba(255, 248, 251, 1) 0%, rgba(255, 228, 240, 1) 50%);
  grid-template-columns: 1fr 250px;
  grid-template-rows: auto auto auto auto auto;
  gap: 1rem;
  padding: 1rem;
  min-height: 100vh;
  box-sizing: border-box;
}

.title {
  font-family: 'Comic Sans MS', 'Baloo 2', cursive;
  margin-bottom: 2rem;
  color: #2c3e50;
  grid-column: 1 / 3;
  grid-row: 1 / 5;
  text-align: center;
  color: #2c3e50;
  font-size: 3rem;
  letter-spacing: 1.5px;
  text-shadow: 2px 2px 4px rgba(0,0,0,0.1);
}

.sidebar {
  grid-column: 2 / 3;
  grid-row: 2 / 5;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.instructions, .records {
  padding: 1rem;
  border: 1px solid #caa0d4; /* 淡紫 */
  background-color: rgba(255, 240, 245, 0.5);
  border-radius: 8px;
  width: 300px;
  /* text-align: center; */
  font-family: 'Comic Sans MS', 'Baloo 2', cursive;

}

.instructions h2,
.records {
  text-align: center;
  margin-bottom: 0.5rem;
}
.buttons {
  grid-column: 1 / 3;
  grid-row: 3 / 4;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding-top: 2rem;
}

.button{
  width: 150px;
  height: 60px;
  font-size: 1.6rem;
  font-family: 'Comic Sans MS', 'Baloo 2', cursive;
  margin-top: 100px;
}

.lives {
  grid-column: 1 / 2;
  grid-row: 2 / 5;
  display: flex;
  left: 12px;
  bottom: 12px;
  display: flex;
  gap: 3px;
  margin-top: 40px; 
}
.heart {
  width: 25px;
  height: 25px;
}
.text {
  font-size: 13px;
  line-height: 1.6;
  display: flex;
  align-items: flex-start;
  text-align: left;
}

.text::before {
  content: attr(data-icon);
  display: inline-block;
  width: 1.8em;
  flex-shrink: 0;
}
</style>