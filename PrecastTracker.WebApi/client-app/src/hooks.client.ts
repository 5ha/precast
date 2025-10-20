import { dev } from '$app/environment';

// Initialize MSW in development mode
if (dev && typeof window !== 'undefined') {
	const { worker } = await import('./mocks/browser');
	worker.start({
		onUnhandledRequest: 'bypass' // Don't warn about unhandled requests
	});
}
