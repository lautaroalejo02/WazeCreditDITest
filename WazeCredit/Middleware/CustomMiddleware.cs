using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WazeCredit.Service.LifeTimeExample;

namespace WazeCredit.Middleware
{
    public class CustomMiddleware
    {
        private readonly RequestDelegate _next;
        //En las lineas comentadas a continuacion veremos como se usa la injeccion en el middleware but we have to be careful becauses this kind of injection are always singletons. No matter what we have specified in services.
        //private readonly TransientService transientService;
        public CustomMiddleware(RequestDelegate next
            //TransientService transientService;
            
            )
        {
            _next = next;
            //_transientService = transientService
        }

        public async Task InvokeAsync(HttpContext context,TransientService transientService,
            ScopedService scopedService,SingletonService singletonService)
        {
            context.Items.Add("CustomMiddlewareTransient", "Transient Middleware - " + transientService.GetGuid());
            context.Items.Add("CustomMiddlewareScoped", "Scoped Middleware - " + scopedService.GetGuid());
            context.Items.Add("CustomMiddlewareSingleton", "Singleton Middleware - " + singletonService.GetGuid());

            await _next(context);
        }
    }
}
