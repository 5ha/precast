import adapter from '@sveltejs/adapter-static';
import { vitePreprocess } from '@sveltejs/vite-plugin-svelte';

/** @type {import('@sveltejs/kit').Config} */
const config = {
	preprocess: vitePreprocess(),

	kit: {
		adapter: adapter({
			// Output to the .NET project's wwwroot directory
			pages: '../wwwroot',
			assets: '../wwwroot',
			fallback: 'index.html',
			precompress: false
		}),
		prerender: {
			entries: [],
			handleHttpError: ({ path, referrer, message, status }) => {
				// Skip prerendering dynamic routes with parameters
				if (path.includes('[') && path.includes(']')) {
					throw new Error('Dynamic route encountered during prerendering');
				}

				// Ignore 404s for dynamic routes during prerendering
				if (status === 404) {
					console.warn(`Prerendering 404: ${path} (referrer: ${referrer || 'none'})`);
					return;
				}

				throw new Error(`Prerendering failed for ${path}: ${message} (status: ${status})`);
			}
		}
	}
};

export default config;
