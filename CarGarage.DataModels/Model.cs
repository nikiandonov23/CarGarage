using System;
using System.Collections.Generic;

namespace CarGarage.DataModels;

public partial class Model
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime? CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }



    public int MakeId { get; set; }
    public virtual Make Make { get; set; } = null!;

}
