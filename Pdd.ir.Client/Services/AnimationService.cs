using Microsoft.JSInterop;

namespace Pdd.ir.Client.Services
{
    public class AnimationService
    {
        private readonly IJSRuntime _js;

        public AnimationService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task InitScrollAnimationsAsync()
        {
            try { await _js.InvokeVoidAsync("initScrollAnimations"); } catch { }
        }

        public async Task InitMouseParallaxAsync()
        {
            try { await _js.InvokeVoidAsync("initMouseParallax"); } catch { }
        }

        public async Task InitCountersAsync()
        {
            try { await _js.InvokeVoidAsync("initCounters"); } catch { }
        }

        public async Task InitHeroAnimationAsync()
        {
            try { await _js.InvokeVoidAsync("initHeroAnimation"); } catch { }
        }

        public async Task InitTiltEffectAsync()
        {
            try { await _js.InvokeVoidAsync("initTiltEffect"); } catch { }
        }

        public async Task AnimateOnScrollAsync(string selector)
        {
            try { await _js.InvokeVoidAsync("animateOnScroll", selector); } catch { }
        }

        public async Task StaggerRevealAsync(string selector, double staggerDelay = 0.1)
        {
            try { await _js.InvokeVoidAsync("staggerReveal", selector, staggerDelay); } catch { }
        }

        public async Task CountUpAsync(string elementId, int target, int duration = 2000)
        {
            try { await _js.InvokeVoidAsync("countUp", elementId, target, duration); } catch { }
        }
    }
}
