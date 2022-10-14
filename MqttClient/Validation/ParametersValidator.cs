using System.Data;
using FluentValidation;
using MqttClient.AppConfiguration;

namespace MqttClient.Validation;

public class ParametersValidator : AbstractValidator<ClientParameters>
{
    public ParametersValidator()
    {
        RuleFor(x => x.LastWillMessage)
            .NotEmpty()
            .When(x => x.WithWill);

        RuleFor(x => x.LastWillTopic)
            .NotEmpty()
            .When(x => x.WithWill);

        RuleFor(x => x.LastWillQos)
            .InclusiveBetween(0, 2)
            .When(x => x.WithWill);
    }
}