namespace InteractiveGameApi.InteractiveGame.DAL.Entities
{
    public class PotentiometerResult
    {
        public int Id { get; set; }
        public DateTime SessionDate { get; set; }
        public int TotalMistakes { get; set; }
        public string? LastStatus { get; set; }
    }

}
