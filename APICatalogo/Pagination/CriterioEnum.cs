using System.ComponentModel.DataAnnotations;

namespace APICatalogo.Pagination;

public enum CriterioEnum
{
    [Display(Name = "Maior")]
    Maior,
    [Display(Name = "Menor")]   
    Menor,
    [Display(Name = "Igual")]  
    Igual
}