<script lang="ts">
	import { onMount } from 'svelte';
	import { ApiClient, TestCylinderQueueResponse, SwaggerException } from '$lib/api/generated/api-client';
	import TestSetDayModal from './TestSetDayModal.svelte';

	let tests = $state<TestCylinderQueueResponse[]>([]);
	let loading = $state(true);
	let error = $state<string | null>(null);
	let selectedTestSetDayId = $state<number | null>(null);
	let isModalOpen = $state(false);

	async function loadTests() {
		loading = true;
		error = null;
		try {
			const client = new ApiClient();
			tests = await client.testsDueToday();
		} catch (e) {
			if (e instanceof SwaggerException) {
				error = `Error loading tests: ${e.message}`;
			} else {
				error = 'An unexpected error occurred';
			}
		} finally {
			loading = false;
		}
	}

	onMount(async () => {
		await loadTests();
	});

	function handleRowClick(testSetDayId: number | undefined) {
		if (testSetDayId) {
			selectedTestSetDayId = testSetDayId;
			isModalOpen = true;
		}
	}

	function handleModalClose() {
		isModalOpen = false;
		selectedTestSetDayId = null;
	}

	async function handleModalSuccess() {
		// Refresh the test list after successful save
		await loadTests();
	}

	function formatDate(date: Date | undefined): string {
		if (!date) return '';
		return new Date(date).toLocaleDateString();
	}

	function formatCylinder(dayNum: number | undefined): string {
		if (!dayNum) return '';
		return `${dayNum}C`;
	}
</script>

<div class="todays-test-queue">
	<h2>Today's Testing Queue</h2>

	{#if loading}
		<p>Loading tests...</p>
	{:else if error}
		<p class="error">{error}</p>
	{:else if tests.length === 0}
		<p>No tests due today</p>
	{:else}
		<table>
			<thead>
				<tr>
					<th>Test Code</th>
					<th>Cylinder</th>
					<th>Job</th>
					<th>Mix</th>
					<th>Required PSI</th>
					<th>Piece Type</th>
					<th>Cast Date</th>
				</tr>
			</thead>
			<tbody>
				{#each tests as test}
					<tr class="clickable" onclick={() => handleRowClick(test.testSetDayId)}>
						<td>{test.testCylinderCode}</td>
						<td>{formatCylinder(test.dayNum)}</td>
						<td>{test.jobCode} - {test.jobName}</td>
						<td>{test.mixDesignCode}</td>
						<td>{test.requiredPsi}</td>
						<td>{test.pieceType}</td>
						<td>{formatDate(test.castDate)}</td>
					</tr>
				{/each}
			</tbody>
		</table>
	{/if}
</div>

<TestSetDayModal
	testSetDayId={selectedTestSetDayId}
	open={isModalOpen}
	onClose={handleModalClose}
	onSuccess={handleModalSuccess}
/>

<style>
	.todays-test-queue {
		padding: 1rem;
	}

	h2 {
		margin-bottom: 1rem;
	}

	table {
		width: 100%;
		border-collapse: collapse;
	}

	th,
	td {
		padding: 0.5rem;
		text-align: left;
		border: 1px solid #ddd;
	}

	th {
		background-color: #f4f4f4;
		font-weight: bold;
	}

	tbody tr:hover {
		background-color: #f9f9f9;
	}

	tbody tr.clickable {
		cursor: pointer;
	}

	tbody tr.clickable:hover {
		background-color: #e3f2fd;
	}

	.error {
		color: red;
	}
</style>
