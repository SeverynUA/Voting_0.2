namespace Voting_0._2.Models.ViewModels.EditModel
{
    public class CandidateEditModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile ImageFile { get; set; }
    }
}
