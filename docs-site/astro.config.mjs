// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import alxGrammar from '../syntaxes/ALX.tmLanguage.json' with { type: 'json' };

// https://astro.build/config
export default defineConfig({
	site: 'https://real-devalex.github.io',
	base: '/alx',
	integrations: [
		starlight({
			title: 'ALX',
			description: 'ALX is a beginner-first programming language for making games, and the home of the Alexion Engine.',
			logo: { src: './src/assets/logo.svg', alt: 'ALX' },
			favicon: '/favicon.svg',
			customCss: ['./src/styles/custom.css'],
			social: [{ icon: 'github', label: 'GitHub', href: 'https://github.com/Real-devalex/alx' }],
			editLink: { baseUrl: 'https://github.com/Real-devalex/alx/edit/main/docs-site/' },
			lastUpdated: true,
			expressiveCode: { shiki: { langs: [{ ...alxGrammar, name: 'alx' }] } },
			sidebar: [
				{ label: 'Learn', items: [{ autogenerate: { directory: 'learn' } }] },
				{ label: 'Reference', items: [{ autogenerate: { directory: 'reference' } }] },
				{ label: 'Examples', items: [{ slug: 'examples' }] },
				{ label: 'Alexion Engine', items: [{ autogenerate: { directory: 'engine' } }] },
				{ label: 'Changelog', items: [{ slug: 'changelog' }] },
				{ label: 'Devlog', items: [{ autogenerate: { directory: 'devlog' } }] },
			],
		}),
	],
});
