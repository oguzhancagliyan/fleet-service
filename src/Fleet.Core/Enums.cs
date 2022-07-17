using System.ComponentModel.DataAnnotations;
using System.Reflection;

public enum PackageStatuses
{
    [Display(Name = "Created")]
    Created = 1,

    [Display(Name = "Loaded into Bag")]
    LoadedIntoBag = 2,

    [Display(Name = "Loaded")]
    Loaded = 3,

    [Display(Name = "Unloaded")]
    Unloaded = 4,


    Empty = 5
}

public enum BagStatuses
{
    [Display(Name = "Created")]
    Created = 1,

    [Display(Name = "Loaded")]
    Loaded = 3,

    [Display(Name = "Unloaded")]
    Unloaded = 4,

    Empty = 5
}

public enum ShiptmenUnloadOption
{
    Bag = 1,
    PackageInBag = 2,
    PackageNotInBag = 3,
}