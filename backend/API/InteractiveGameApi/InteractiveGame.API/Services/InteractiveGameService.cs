using System;
using System.IO.Ports;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using InteractiveGameApi.InteractiveGame.BLL;
using InteractiveGameApi.InteractiveGame.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace InteractiveGameApi.InteractiveGame.API.Services
{
    public class InteractiveGameService : IDisposable
    {
        private readonly PotentiometerService _potService;
        private readonly IHubContext<GameHub> _hub;
        private readonly SerialPort _serialPort;

        private readonly SequenceGameState<string> _game1State;
        private readonly SequenceGameState<string> _game2State;
        private readonly SequenceGameState<string> _game3State;
        private readonly SequenceGameState<string> _game4State;
        private readonly SequenceGameState<string> _game5State;

        public InteractiveGameService(PotentiometerService potService, IHubContext<GameHub> hubContext)
        {
            _potService = potService;
            _hub = hubContext;

            _game1State = new SequenceGameState<string>(3);
            _game2State = new SequenceGameState<string>(3);
            _game3State = new SequenceGameState<string>(3);
            _game4State = new SequenceGameState<string>(3);
            _game5State = new SequenceGameState<string>(3);

            _serialPort = new SerialPort("COM4", 9600, Parity.None, 8, StopBits.One)
            {
                NewLine = "\n",
                ReadTimeout = 500,
                DtrEnable = true,
                RtsEnable = false
            };
        }

        private async Task HandleGame1Async()
        {
            _serialPort.ReadTimeout = 3000;

            while (_game1State.Lives > 0)
            {
                Console.WriteLine("Waiting for sequence from serial...");
                Console.WriteLine(_game1State.Lives);
                try
                {
                    string line = _serialPort.ReadLine().Trim();
                    Console.WriteLine($"Serial {line}");

                    if (line.StartsWith("start ", StringComparison.OrdinalIgnoreCase)
                        || line.StartsWith("Initiating new game with", StringComparison.OrdinalIgnoreCase)) continue;


                    if (line == "true") await _hub.Clients.All.SendAsync("sequenceResult", true);

                    if (line == "false")
                    {
                        await _hub.Clients.All.SendAsync("sequenceResult", false); 
                        _game1State.Lives--;
                    }
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("Serial read timed out. Retrying...");
                }
            }

            if(_game1State.Lives == 0)
            {
                HandleQuit(1);
            }
        }

        private async Task HandleGame5Async(SequenceGameState<string> gameState, Func<string, string> parseToken)
        {
            _serialPort.ReadTimeout = 3000;

            gameState.Sequence.Clear();
            gameState.PendingSequence = null;
            gameState.Lives = 3;

            while (gameState.Lives > 0)
            {
                Console.Write("LIVES: ");
                Console.WriteLine(gameState.Lives);
                _serialPort.DiscardInBuffer();
                Console.WriteLine("Waiting for sequence from serial…");

                string[] tokens = null;
                while (true)
                {
                    try
                    {
                        await Task.Delay(1000);
                        string line = _serialPort.ReadLine().Trim();
                        Console.WriteLine("LINE → " + line);

                        if (line.StartsWith("start") || line == "true" || line == "false" || line.StartsWith("art") || line.StartsWith("3") || line.StartsWith("tart") || line.StartsWith("rt")
                            || line.StartsWith("t") || line.StartsWith("2") || line.StartsWith("4") || line.StartsWith("Unknown") || line.StartsWith("input") || line.StartsWith("5"))
                            continue;

                        tokens = line
                            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        Console.WriteLine("tokens: [" + string.Join(", ", tokens) + "]");

                        if (tokens.Length > 0)
                        {
                            Console.WriteLine("Valid sequence received.");
                            break;
                        }
                        else
                        {
                            Console.WriteLine("No valid sequence received. Waiting…");
                        }
                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine("Serial read timed out. Retrying…");
                    }
                }

                string first = tokens[0];
                int count = tokens.Count(t => t == first);
                Console.WriteLine($"First element '{first}' appears {count} time(s)");

                gameState.PendingSequence = null;
                gameState.Sequence = new List<string> { count.ToString() };
                Console.WriteLine("Expected answer: " + count);

                bool correctAnswerReceived = false;
                while (!correctAnswerReceived)
                {
                    Console.WriteLine("Waiting for user input from web…");

                    while (gameState.PendingSequence == null)
                        Thread.Sleep(200);

                    var answer = gameState.PendingSequence
                                         .Where(s => !string.IsNullOrWhiteSpace(s))
                                         .ToArray();
                    Console.WriteLine("User answer: [" + string.Join(", ", answer) + "]");

                    if (answer.Length != 1)
                    {
                        Console.WriteLine("Invalid input length. Expected exactly 1 number.");
                        gameState.PendingSequence = null;
                        continue;
                    }

                    if (answer[0] == count.ToString())
                    {
                        _serialPort.WriteLine("true");
                        Console.WriteLine("Correct! Count matched.");
                        await _hub.Clients.All.SendAsync("sequenceResult", true);
                        gameState.PendingSequence = null;
                        _serialPort.DiscardInBuffer();
                        correctAnswerReceived = true;
                        break; 
                    }
                    else
                    {
                        _serialPort.WriteLine("false");
                        Console.WriteLine($"Incorrect (expected {count}, got {answer[0]}). Lives left: {gameState.Lives}");
                        await _hub.Clients.All.SendAsync("sequenceResult", false);

                        if (gameState.Lives <= 0)
                        {
                            Console.WriteLine("Game over. No lives left.");
                            return;
                        }

                        gameState.PendingSequence = null;
                    }
                }

                gameState.Sequence.Clear();
            }

            if(gameState.Lives <= 0)
            {
                HandleQuit(5);
            }
            Console.WriteLine("Game over. No lives left.");
        }

        private async Task HandleGenericGameAsync<T>(SequenceGameState<T> gameState, Func<string, T> parseToken, int gameID)
        {
            _serialPort.ReadTimeout = 3000;

            gameState.Sequence.Clear();
            gameState.PendingSequence = null;
            gameState.Lives = 3;


            while (gameState.Lives > 0)
            {
                _serialPort.DiscardInBuffer();
                Console.WriteLine("Waiting for sequence from serial...");

                string[] tokens = null;

                while (true)
                {
                    try
                    {
                        await Task.Delay(1000);
                        string line = _serialPort.ReadLine().Trim();
                        Console.Write("LINE ");
                        Console.WriteLine(line);

                        if (line.StartsWith("start") || line == "true" || line == "false" || line.StartsWith("art") || line.StartsWith("3\\") || line.StartsWith("tart") || line.StartsWith("rt")
                            || line.StartsWith("t") || line.StartsWith("2\\") || line.StartsWith("4\\") 
                            || line.StartsWith("Unknown") || line.StartsWith("input") || line.StartsWith("ue")
                            || line.StartsWith("rue") || line.StartsWith("e") || line.StartsWith("alse") || line.StartsWith("lse")
                            || line.StartsWith("se") || line.StartsWith("quit") || line.StartsWith("uit") || line.StartsWith("it"))
                            continue;

                        tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        Console.WriteLine("tokens: [" + string.Join(", ", tokens) + "]");

                        if (tokens.Length > 0)
                        {
                            Console.WriteLine("Valid sequence received.");
                            break;
                        }
                        else
                        {
                            Console.WriteLine("No valid sequence received. Waiting...");
                        }
                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine("Serial read timed out. Retrying...");
                    }
                }

                gameState.PendingSequence = new List<string>();
                gameState.Sequence = tokens.Select(parseToken).ToList();
                Console.WriteLine("Sequence received " + string.Join(", ", gameState.Sequence));

                bool correctAnswerReceived = false;

                while (!correctAnswerReceived)
                {
                    Console.WriteLine("Waiting for user input from web...");
                    Console.Write("gameState.Lives ");
                    Console.WriteLine(gameState.Lives);

                    while (gameState.PendingSequence == null)
                    {
                        Thread.Sleep(200);
                    }

                    if (gameState.PendingSequence.Count == 1 &&
                        string.Equals(gameState.PendingSequence[0], "quit", StringComparison.OrdinalIgnoreCase))
                    {
                        _serialPort.WriteLine("quit");
                        Console.WriteLine("User quit the game.");
                        gameState.PendingSequence = null;
                        return;
                    }

                    Console.Write("vdlud: ");
                    Console.WriteLine(string.Join(" ", gameState.PendingSequence
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList()));

                    gameState.PendingSequence = gameState.PendingSequence
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList();

                    Console.WriteLine(tokens.Length);
                    Console.WriteLine(gameState.PendingSequence.Count);

                    if (gameState.PendingSequence.Count != tokens.Length)
                    {
                        Console.WriteLine("Invalid input length.#3");
                        gameState.PendingSequence = null;
                        continue;
                    }

                    T[] userSequence;
                    try
                    {
                        userSequence = gameState.PendingSequence.Select(parseToken).ToArray();
                    }
                    catch
                    {
                        Console.WriteLine("Input conversion failed.");
                        gameState.PendingSequence = null;
                        continue;
                    }

                    var (correct, lives) = EvaluateSequenceGuess2(gameState, userSequence);
                    Console.WriteLine(correct);

                    if (correct)
                    {
                        Console.WriteLine("fdgfdgdf");
                        Console.WriteLine("Correct! Sequence matched.");

                        await _hub.Clients.All.SendAsync("sequenceResult", true);

                        gameState.PendingSequence = null;
                        _serialPort.DiscardInBuffer();

                        correctAnswerReceived = true;
                        correct = false;
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Incorrect sequence.");

                        await _hub.Clients.All.SendAsync("sequenceResult", false);

                        if (lives == 0)
                        {
                            Console.WriteLine("Game over. No lives left.");
                            gameState.PendingSequence = null;
                            return;
                        }

                        gameState.PendingSequence = null;
                    }

                    //_serialPort.WriteLine(correct ? "true" : "false");
                }

                gameState.Sequence.Clear(); // Prepare for next round
            }

            if (gameState.Lives <= 0)
            {
                HandleQuit(gameID);
            }
            Console.WriteLine("Game over. No lives left.");
        }

        private (bool correct, int lives) EvaluateSequenceGuess2<T>(SequenceGameState<T> state, T[] guess)
        {
            bool correct = guess.SequenceEqual(state.Sequence);
            Console.WriteLine(string.Join(", ", guess));
            Console.WriteLine(string.Join(", ", state.Sequence));

            return (correct, state.Lives);
        }

        private (bool correct, int lives) EvaluateSequenceGuess<T>(SequenceGameState<T> state, T[] guess)
        {
            bool correct = guess.SequenceEqual(state.Sequence);
            Console.WriteLine(string.Join(", ", guess));
            Console.WriteLine(string.Join(", ", state.Sequence));

            if (!correct) state.Lives--;
            return (correct, state.Lives);
        }

        public (bool Correct, int Lives) SubmitSequence(int gameId, List<string> sequence)
        {
            switch (gameId)
            {
                case 2:
                    {
                        // 1) Stash the user’s answer so the game loop can pick it up
                        _game2State.PendingSequence = new List<string>(sequence);

                        // 2) Evaluate it
                        var parsed = sequence.ToArray();
                        var result = EvaluateSequenceGuess(_game2State, parsed);

                        // 3) Send the result back over serial
                        _serialPort.WriteLine(result.correct ? "true" : "false");

                        return result;
                    }

                case 3:
                    {
                        _game3State.PendingSequence = new List<string>(sequence);
                        var parsed = sequence.ToArray();
                        var result = EvaluateSequenceGuess(_game3State, parsed);
                        _serialPort.WriteLine(result.correct ? "true" : "false");
                        return result;
                    }

                case 4:
                    {
                        _game4State.PendingSequence = new List<string>(sequence);
                        var parsed = sequence.ToArray();
                        var result = EvaluateSequenceGuess(_game4State, parsed);
                        _serialPort.WriteLine(result.correct ? "true" : "false");
                        return result;
                    }

                case 5:
                    {
                        _game5State.PendingSequence = new List<string>(sequence);
                        var parsed = sequence.ToArray();
                        var result = EvaluateSequenceGuess(_game5State, parsed);
                        return result;
                    }

                default:
                    throw new ArgumentException("Invalid gameId");
            }
        }

        public void Dispose() => _serialPort?.Dispose();

        public async Task<bool> StartGame(int selectedGame)
        {
            try
            {
                if (!_serialPort.IsOpen)
                {
                    _serialPort.Open();
                    await Task.Delay(3000);
                }
                
                _serialPort.DiscardInBuffer();           

                string startCommand = $"start {selectedGame}\\n\\r";
                Console.WriteLine($"Serial {startCommand}");
                _serialPort.WriteLine("quit");
                _serialPort.WriteLine(startCommand);    

                _ = Task.Run(async () =>
                {
                    try
                    {
                        switch (selectedGame)
                        {
                            case 1:
                                await HandleGame1Async();
                                break;
                            case 2:
                                await HandleGenericGameAsync(_game2State, s => s, 2);
                                break;
                            case 3:
                                await HandleGenericGameAsync(_game3State, s => s, 3);
                                break;
                            case 4:
                                await HandleGenericGameAsync(_game4State, s => s, 4);
                                break;
                            case 5:
                                await HandleGame5Async(_game5State, s => s);
                                break;
                            default:
                                Console.WriteLine("Invalid game selected.");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Game handler exception: {ex.Message}");
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StartGame error: {ex.Message}");
                return false;
            }
        }

        public void HandleQuit(int gameId)
        {
            SequenceGameState<string> state = gameId switch
            {
                1 => _game1State,
                2 => _game2State,
                3 => _game3State,
                4 => _game4State,
                5 => _game5State,
                _ => throw new ArgumentException($"Invalid gameId: {gameId}")
            };
            state.Reset();

            Console.WriteLine($"Serial quit");
            _serialPort.WriteLine("quit");
        }

    }

    public class SequenceGameState<T>
    {
        public List<T> Sequence { get; set; } = new();
        public int Lives { get; set; }
        public List<string> PendingSequence { get; set; } = null;
        public SequenceGameState(int lives)
        {
            Lives = lives;
        }

        public void Reset()
        {
            Sequence.Clear();
            Lives = 3;
            PendingSequence = null;
        }
    }
}
