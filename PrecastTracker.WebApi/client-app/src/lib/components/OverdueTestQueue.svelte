<script lang="ts">
	import { onMount } from 'svelte';
	import { ApiClient, TestCylinderQueueResponse, SwaggerException } from '$lib/api/generated/api-client';

	let tests = $state<TestCylinderQueueResponse[]>([]);
	let loading = $state(true);
	let error = $state<string | null>(null);

	onMount(async () => {
		try {
			const client = new ApiClient();
			tests = await client.testsOverdue();
		} catch (e) {
			if (e instanceof SwaggerException) {
				error = `Error loading tests: ${e.message}`;
			} else {
				error = 'An unexpected error occurred';
			}
		} finally {
			loading = false;
		}
	});

	function formatDate(date: Date | undefined): string {
		if (!date) return '';
		return new Date(date).toLocaleDateString();
	}

	function formatCylinder(dayNum: number | undefined): string {
		if (!dayNum) return '';
		return `${dayNum}C`;
	}

	function formatOverdue(dateDue: Date | undefined): string {
		if (!dateDue) return '';

		const now = new Date();
		const due = new Date(dateDue);
		const diffMs = now.getTime() - due.getTime();

		if (diffMs < 0) return '';

		const totalHours = Math.floor(diffMs / (1000 * 60 * 60));
		const days = Math.floor(totalHours / 24);
		const hours = totalHours % 24;

		if (days > 0) {
			return `${days}d ${hours}h`;
		} else {
			return `${hours}h`;
		}
	}
</script>

<div class="overdue-test-queue">
	<h2>Overdue Tests</h2>

	{#if loading}
		<p>Loading tests...</p>
	{:else if error}
		<p class="error">{error}</p>
	{:else if tests.length === 0}
		<p>No overdue tests</p>
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
					<th>Days Overdue</th>
				</tr>
			</thead>
			<tbody>
				{#each tests as test}
					<tr>
						<td>{test.testCylinderCode}</td>
						<td>{formatCylinder(test.dayNum)}</td>
						<td>{test.jobCode} - {test.jobName}</td>
						<td>{test.mixDesignCode}</td>
						<td>{test.requiredPsi}</td>
						<td>{test.pieceType}</td>
						<td>{formatDate(test.castDate)}</td>
						<td>{formatOverdue(test.dateDue)}</td>
					</tr>
				{/each}
			</tbody>
		</table>
	{/if}
</div>

<style>
	.overdue-test-queue {
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

	.error {
		color: red;
	}
</style>
