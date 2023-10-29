using Discord.Interactions;
using Discord;

namespace Autoposter.BusinessLayer.Models
{
    public class CreateAdviceModel : IModal
    {
        public string Title => "Ваше обьявление";
        [InputLabel("Ваш игровой ник")]
        [ModalTextInput("nickname", TextInputStyle.Short, maxLength: 100)]
        public string? Nickname { get; set; }

        [InputLabel("Описание")]
        [ModalTextInput("description", TextInputStyle.Paragraph, maxLength: 1000)]
        public string? Description { get; set; }
    }
}
