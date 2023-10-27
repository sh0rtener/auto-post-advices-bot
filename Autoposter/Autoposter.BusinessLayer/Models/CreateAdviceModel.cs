using Discord.Interactions;
using Discord;

namespace Autoposter.BusinessLayer.Models
{
    public class CreateAdviceModel : IModal
    {
        public string Title => "Создать обьявление";
        [InputLabel("Ваш игровой ник")]
        [ModalTextInput("nickname", TextInputStyle.Short, placeholder: "Ivan Ivanov", maxLength: 100)]
        public string? Nickname { get; set; }

        [InputLabel("Описание аккаунта")]
        [ModalTextInput("description", TextInputStyle.Paragraph,
            placeholder: "Здесь вы должны описать свой аккаунт, указать игры и тому подобное, указать цену", maxLength: 1000)]
        public string? Description { get; set; }
    }
}
