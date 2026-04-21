namespace CarGarage.DataModels;

public partial class Make
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime? CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }


    public virtual ICollection<Model> Models { get; set; } = new List<Model>();

}
