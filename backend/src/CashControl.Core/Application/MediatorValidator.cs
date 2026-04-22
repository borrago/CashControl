using FluentValidation;

namespace CashControl.Core.Application;

public class MediatorValidator<TMediatorInput> : AbstractValidator<TMediatorInput>, IMediatorValidator;