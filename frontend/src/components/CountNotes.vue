<template>
  <div class="count-notes">
      <h1 class="title">Count Notes</h1>
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
          <p class="text" data-icon="👂">Listen to the note sequence played in the game.</p>
          <p class="text" data-icon="🔢">Count how many times the "first“ note appears in the sequence.</p>
          <p class="text" data-icon="🧠">Enter your answer in the input box and click the Submit button.</p>
          <p class="text" data-icon="🎵">You can click and play the notes at the bottom to help you remember.</p>
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
    
      <div class="question-bar">
        <p style="font-size: 16px;font-family: 'Comic Sans MS', 'Baloo 2', cursive;">How many times did you hear the "first" note in the sequence?</p>
        <div class="input-group">
            <el-input v-model="input" placeholder="Input Box" style="width: 140px;"></el-input>
            <el-button class="submit-button" type="primary" @click="submitAnswer">Submit</el-button>
        </div>
      </div>
      <div class="colorButtons">
        <el-row>
          <el-button class="colorButton" @click="playNote('do')" >Do</el-button>
          <el-button class="colorButton" @click="playNote('re')">Re</el-button>
          <el-button class="colorButton" @click="playNote('mi')">Mi</el-button>
          <el-button class="colorButton" @click="playNote('fa')">Fa</el-button>
          <el-button class="colorButton" @click="playNote('sol')">So</el-button>
          <el-button class="colorButton" @click="playNote('la')">La</el-button>
          <el-button class="colorButton" @click="playNote('si')">Si</el-button>
        </el-row>
        
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
      input:'',
      totalLives : 3,
      heartIcon : '../public/icon/heart1.png',
      brokenheartIcon: '../public/icon/heart2.png',
      gameName: 'Count Notes',
      errorCount: 0,
      elapsedSeconds: 0,
      timer: null,
      gameId: 5,
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
     playNote(note) {
      const noteFrequencies = {
    'do': 261.63,
    're': 293.66,
    'mi': 329.63,
    'fa': 349.23,
    'sol': 392.00,
    'la': 440.00,
    'si': 493.88
    };

    const frequency = noteFrequencies[note.toLowerCase()];
    if (!frequency) return;
   
    const audioCtx = new (window.AudioContext || window.webkitAudioContext)();
    const oscillator = audioCtx.createOscillator();
    const gainNode = audioCtx.createGain();
   
    oscillator.type = 'square'; 
    oscillator.frequency.setValueAtTime(frequency, audioCtx.currentTime); 
    gainNode.gain.setValueAtTime(0.2, audioCtx.currentTime); 
   
    oscillator.connect(gainNode);
    gainNode.connect(audioCtx.destination);
   
    oscillator.start();
    oscillator.stop(audioCtx.currentTime + 0.3); 
  
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
      console.log(result,'result');
    if(result == false){
          this.errorCount++
    }
    if(this.errorCount >= 3 && !this.hasGameOver){
      this.hasGameOver = true;
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
    async submitSequence(gameId, sequenceArray) {
    try {
        const response = await fetch(`/api/game/submit-sequence/${gameId}`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(sequenceArray)
        });

        const result = await response.json();
        console.log("Submit result:", result); // { correct: true/false }
    } catch (err) {
        console.error("Error submitting sequence:", err);
    }
},
    async submitAnswer() {
    if (!this.isRunning) {
      this.$alert('Please start the game first!', {
      confirmButtonText: 'OK'
    });
      return;
    }
    if (!this.input) {
      this.$alert('Complete the input to submit!', {
      confirmButtonText: 'OK'
    });
      return;
    }
    let answer = this.input;
    answer = [answer];
    console.log(answer,"input")
    await this.submitSequence(this.gameId, answer);
    this.input = ''
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
    }
  },
}
</script>
<style scoped>
.count-notes {
  display: grid;
  /* background: linear-gradient(to bottom, #fdfbfb, #ebedee); */
   background: radial-gradient(circle,rgba(255, 248, 251, 1) 0%, rgba(255, 228, 240, 1) 50%);
  grid-template-columns: 1fr 250px;
  grid-template-rows: auto auto auto auto auto auto;
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
  border: 1px solid #caa0d4; 
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
  grid-row:  4 / 6;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding-top: 2rem;
}

.colorButtons {
  grid-column: 1 / 3;
  grid-row:  3 / 6 ;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding-top: 1.5rem;
}

.button{
  width: 150px;
  height: 60px;
  font-size: 1.6rem;
  font-family: 'Comic Sans MS', 'Baloo 2', cursive;
  margin-top: -150px;

}

.colorButton {
  margin-right: -12px;
  margin-bottom: 2px;
  width: 50px;
  height: 90px;
  font-size: 16px;
  padding: 20px 20px;
  text-align: center;
  font-family: 'Comic Sans MS', 'Baloo 2', cursive;

}
.submit-button{
    font-family: 'Comic Sans MS', 'Baloo 2', cursive;

}
.question-bar {
  grid-column: 1 / 3;
  grid-row:  2 / 6 ;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding-top: 2rem;
  margin-top: 60px;
}
.input-group {
    display: flex;
    gap: 15px;
    margin-top: 10px;
}
.lives {
  grid-column: 1 / 2;
  grid-row: 2 / 5;
  display: flex;
  left: 12px;
  bottom: 12px;
  display: flex;
  gap: 3px;
  margin-top: 50px;
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
.submit-button{
    font-family: 'Comic Sans MS', 'Baloo 2', cursive;

}
</style>