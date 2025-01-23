using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Entities
{
    public class BaseEntity : IDataChangeStatEntity
    {

        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public DateTime? CreationDate { get; set; }

        [MaxLength(256)]
        public string? ChangeUser { get; set; }

    }



    public interface IDataChangeStatEntity
    {
        DateTime? ModifiedDate { get; set; }
        DateTime? CreationDate { get; set; }
        string? ChangeUser { get; set; }
    }
}
