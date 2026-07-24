using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarGarage.DataModels.Enums
{
    public enum PaymentMethod
    {
        [Display(Name = "В брой")]
        Cash = 0,

        [Display(Name = "С карта")]
        Card = 1,

        [Display(Name = "По банков път")]
        BankTransfer = 2
    }
}
