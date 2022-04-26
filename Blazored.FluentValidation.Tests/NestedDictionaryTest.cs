using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Blazored.FluentValidation.Tests;

public class NestedDictionaryTest
{
    private readonly ServiceProvider _provider;

    public NestedDictionaryTest()
    {
        var services = new ServiceCollection();
        services.AddValidatorsFromAssemblyContaining<NestedDictionaryTest>();
        _provider = services.BuildServiceProvider();
    }

    [Fact]
    public void Test()
    {
        var parent = new Parent
        {
            Items = new Dictionary<int, Item>()
            {
                [1] = new()
            }
        };
        var expectedValidity = false;

        var validationResults = new List<ValidationResult>();
        var validators = _provider.GetServices<IValidator<Parent>>();
        foreach (var validator in validators)
        {
            validationResults.Add(validator.Validate(parent));
        }

        var manualValidityResult = validationResults.All(vr => vr.IsValid);

        var editContext = new EditContext(parent);
        editContext.AddFluentValidation(_provider, true, null, null);
        var blazoredValidityResult = editContext.Validate();

        using (new AssertionScope())
        {
            manualValidityResult.Should().Be(expectedValidity);
            blazoredValidityResult.Should().Be(manualValidityResult);
        }
    }
}

public class Parent
{
    public IReadOnlyDictionary<int, Item> Items { get; set; } = new Dictionary<int, Item>();
}

public class Item
{
    public string Text { get; set; } = "";
}

public class ParentValidator : AbstractValidator<Parent>
{
    public ParentValidator()
    {
        RuleForEach(p => p.Items.Values)
            .SetValidator(new ItemValidator());
    }
}

public class ItemValidator : AbstractValidator<Item>
{
    public ItemValidator()
    {
        RuleFor(i => i.Text)
            .NotEmpty();
    }
}

