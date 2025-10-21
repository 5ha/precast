import { dev } from '$app/environment';

// Initialize MSW in development mode
// Set VITE_USE_MSW=true in .env to enable MSW, or VITE_USE_MSW=false to disable
const useMSW = import.meta.env.VITE_USE_MSW === 'true';

if (dev && typeof window !== 'undefined' && useMSW) {
	const { worker } = await import('./mocks/browser');
	worker.start({
		onUnhandledRequest: 'bypass' // Don't warn about unhandled requests
	});
}
