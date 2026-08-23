namespace InteractiveGameApi.InteractiveGame.BLL
{
    public class PotentiometerService
    {
        private string _lastStatus = "N/A";
        private int _mistakeCount = 0;
        private int _iteration = 1;

        public void StartNewIteration()
        {
            _iteration++;
            _lastStatus = "N/A";
            _mistakeCount = 0;
        }

        public void UpdateStatus(string status)
        {
            _lastStatus = status;
            if (status == "WRONG")
            {
                _mistakeCount++;
            }
        }

        public (string LastStatus, int Mistakes, int Iteration) GetStatus()
        {
            return (_lastStatus, _mistakeCount, _iteration);
        }

        public void Reset()
        {
            _iteration = 1;
            _lastStatus = "N/A";
            _mistakeCount = 0;
        }
    }
}
