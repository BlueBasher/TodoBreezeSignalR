namespace Todo_DurandalBreezeJsSignalR.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class TodoItem
    {
        public int Id { get; set; }

        [Required, StringLength(maximumLength: 30)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool IsDone { get; set; }
        public bool IsArchived { get; set; }
    }
}