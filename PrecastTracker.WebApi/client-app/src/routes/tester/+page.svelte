<script lang="ts">
	import TodaysTestQueue from '$lib/components/TodaysTestQueue.svelte';
	import OverdueTestQueue from '$lib/components/OverdueTestQueue.svelte';
</script>

<h1>Tester Dashboard</h1>

<OverdueTestQueue />

<TodaysTestQueue />
