using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace Voting_0._2.Models.Voting_m.Candidat_m
{
    public class Image
    {
        public int Id { get; set; }

        public byte[]? ImageData { get; set; }

        public string? FilePath { get; set; } // Відносний шлях до файлу зображення

        public int CandidatID { get; set; }

        [ForeignKey(nameof(CandidatID))]
        public Candidat Candidat { get; set; } = default!;
    }
}
