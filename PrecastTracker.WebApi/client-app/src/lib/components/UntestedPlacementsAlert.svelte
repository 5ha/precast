<script lang="ts">
	import { onMount } from 'svelte';
	import { ApiClient, UntestedPlacementResponse, SwaggerException } from '$lib/api/generated/api-client';

	let gaps = $state<UntestedPlacementResponse[]>([]);
	let loading = $state(true);
	let error = $state<string | null>(null);
	const daysBack = 7;

	async function loadGaps() {
		loading = true;
		error = null;
		try {
			const client = new ApiClient();
			gaps = await client.untestedPlacements(daysBack);
		} catch (e) {
			console.error('Error loading untested placements:', e);
			if (e instanceof SwaggerException) {
				error = `Error loading untested placements: ${e.message}`;
			} else if (e instanceof Error) {
				error = `An unexpected error occurred: ${e.message}`;
			} else {
				error = 'An unexpected error occurred';
			}
		} finally {
			loading = false;
		}
	}

	onMount(async () => {
		await loadGaps();
	});

	function formatDate(date: Date | undefined): string {
		if (!date) return '';
		return new Date(date).toLocaleDateString();
	}

	function formatTime(timeSpan: string | undefined): string {
		if (!timeSpan) return '';
		// TimeSpan comes as "HH:MM:SS" from API
		const parts = timeSpan.split(':');
		if (parts.length >= 2) {
			return `${parts[0]}:${parts[1]}`;
		}
		return timeSpan;
	}

	function getDaysAgo(castDate: Date | undefined): number {
		if (!castDate) return 0;
		const today = new Date();
		today.setHours(0, 0, 0, 0);

		const cast = new Date(castDate);
		cast.setHours(0, 0, 0, 0);

		const diffMs = today.getTime() - cast.getTime();
		return Math.floor(diffMs / (1000 * 60 * 60 * 24));
	}

	function getDaysAgoText(castDate: Date | undefined): string {
		const days = getDaysAgo(castDate);
		if (days === 0) return 'Today';
		if (days === 1) return '1 day ago';
		return `${days} days ago`;
	}
</script>

{#if loading}
	<!-- Don't show anything while loading -->
{:else if error}
	<div class="alert error-alert">
		<div class="alert-header">
			<span class="alert-icon">⚠️</span>
			<h3>Error Loading Gap Detection</h3>
		</div>
		<p>{error}</p>
	</div>
{:else if gaps.length > 0}
	<div class="alert gaps-alert">
		<div class="alert-header">
			<span class="alert-icon">⚠️</span>
			<h3>{gaps.length} PLACEMENT{gaps.length === 1 ? '' : 'S'} MISSING TEST CYLINDERS</h3>
		</div>
		<table>
			<thead>
				<tr>
					<th>Pour</th>
					<th>Bed</th>
					<th>Job</th>
					<th>Piece Type</th>
					<th>Mix</th>
					<th>Cast Date</th>
					<th>Cast Time</th>
					<th>Age</th>
				</tr>
			</thead>
			<tbody>
				{#each gaps as gap}
					<tr class:urgent={getDaysAgo(gap.castDate) >= 3}>
						<td>{gap.pourId}</td>
						<td>{gap.bedId}</td>
						<td>{gap.jobCode} - {gap.jobName}</td>
						<td>{gap.pieceType}</td>
						<td>{gap.mixDesignCode}</td>
						<td>{formatDate(gap.castDate)}</td>
						<td>{formatTime(gap.castTime)}</td>
						<td class="age-cell">{getDaysAgoText(gap.castDate)}</td>
					</tr>
				{/each}
			</tbody>
		</table>
	</div>
{/if}

<style>
	.alert {
		margin-bottom: 1.5rem;
		padding: 1rem;
		border-radius: 4px;
		border: 2px solid;
	}

	.gaps-alert {
		background-color: #fff8e1;
		border-color: #ffa726;
	}

	.error-alert {
		background-color: #ffebee;
		border-color: #ef5350;
	}

	.alert-header {
		display: flex;
		align-items: center;
		gap: 0.5rem;
		margin-bottom: 0.75rem;
	}

	.alert-icon {
		font-size: 1.5rem;
	}

	.alert-header h3 {
		margin: 0;
		font-size: 1rem;
		font-weight: 600;
	}

	.error-alert p {
		margin: 0;
		color: #c62828;
	}

	table {
		width: 100%;
		border-collapse: collapse;
		background-color: white;
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
		font-size: 0.9rem;
	}

	tbody tr {
		background-color: white;
	}

	tbody tr:hover {
		background-color: #f5f5f5;
	}

	tbody tr.urgent {
		background-color: #ffebee;
	}

	tbody tr.urgent:hover {
		background-color: #ffcdd2;
	}

	.age-cell {
		font-weight: 500;
	}

	tbody tr.urgent .age-cell {
		color: #c62828;
		font-weight: 600;
	}
</style>
