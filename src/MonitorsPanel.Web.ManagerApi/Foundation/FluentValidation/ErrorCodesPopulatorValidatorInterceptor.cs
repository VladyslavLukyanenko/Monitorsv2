using System.Linq;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace MonitorsPanel.Web.ManagerApi.Foundation.FluentValidation
{
  public class ErrorCodesPopulatorValidatorInterceptor
    : IValidatorInterceptor
  {
    public IValidationContext BeforeAspNetValidation(ActionContext actionContext, IValidationContext commonContext)
    {
      return commonContext;
    }

    public ValidationResult AfterAspNetValidation(ActionContext actionContext, IValidationContext validationContext,
      ValidationResult result)
    {
      if (!result.IsValid)
      {
        foreach (var errorCode in result.Errors.Select(_ => _.ErrorCode))
        {
          actionContext.ModelState.AddModelError(errorCode, "");
        }

        return new ValidationResult();
      }

      return result;
    }
  }
}