using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Features.Updates;
public static class DependencyInjection {

	public static IServiceCollection AddUpdates(this IServiceCollection services) {
		return services.AddTransient<UpdatesDialogViewModel>();
	}

}
