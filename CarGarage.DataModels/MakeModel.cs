using System;
using System.Collections.Generic;

namespace CarGarage.DataModels;

public partial class MakeModel
{
    public int Id { get; set; }

    public int MakeId { get; set; }

    public int ModelId { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }
}
