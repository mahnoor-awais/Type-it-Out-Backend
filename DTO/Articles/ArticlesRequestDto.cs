using System.ComponentModel.DataAnnotations;

namespace Type_It_Out_Backend.DTO.Articles
{
    public class ArticlesRequestDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int AuthorId { get; set; } = 0;

    }
}
