using Microsoft.EntityFrameworkCore;

namespace CashControl.Core.Infra;

public class __EFAutoHistory : AutoHistory
{
    public Guid? LoggedUserId { get; set; }
}