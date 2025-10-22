<script lang="ts">
	import TestQueue from '$lib/components/TestQueue.svelte';
	import UntestedPlacementsAlert from '$lib/components/UntestedPlacementsAlert.svelte';
</script>

<h1>Tester Dashboard</h1>

<UntestedPlacementsAlert />

<TestQueue />
