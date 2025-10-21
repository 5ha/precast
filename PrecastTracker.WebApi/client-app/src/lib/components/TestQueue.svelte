<script lang="ts">
	import { onMount } from 'svelte';
	import { ApiClient, TestCylinderQueueResponse, SwaggerException } from '$lib/api/generated/api-client';
	import TestSetDayModal from './TestSetDayModal.svelte';

	let tests = $state<TestCylinderQueueResponse[]>([]);
	let loading = $state(true);
	let error = $state<string | null>(null);
	let selectedTestSetDayId = $state<number | null>(null);
	let isModalOpen = $state(false);
	let endDate = $state<Date>(calculateDefaultEndDate());

	function calculateDefaultEndDate(): Date {
		const today = new Date();
		const dayOfWeek = today.getDay(); // 0 = Sunday, 5 = Friday

		if (dayOfWeek === 5) {
			// Friday: show through end of Sunday (2 days ahead)
			const endOfSunday = new Date(today);
			endOfSunday.setDate(today.getDate() + 2);
			endOfSunday.setHours(23, 59, 59, 999);
			return endOfSunday;
		} else {
			// Other days: show through end of tomorrow
			const endOfTomorrow = new Date(today);
			endOfTomorrow.setDate(today.getDate() + 1);
			endOfTomorrow.setHours(23, 59, 59, 999);
			return endOfTomorrow;
		}
	}

	async function loadTests() {
		loading = true;
		error = null;
		try {
			const client = new ApiClient();
			tests = await client.testQueue(endDate);
		} catch (e) {
			console.error('Error loading tests:', e);
			if (e instanceof SwaggerException) {
				error = `Error loading tests: ${e.message}`;
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

	function handleEndDateChange(event: Event) {
		const target = event.target as HTMLInputElement;
		const newDate = new Date(target.value);
		newDate.setHours(23, 59, 59, 999); // Set to end of day
		endDate = newDate;
		loadTests();
	}

	function formatDate(date: Date | undefined): string {
		if (!date) return '';
		return new Date(date).toLocaleDateString();
	}

	function formatCylinder(dayNum: number | undefined): string {
		if (!dayNum) return '';
		return `${dayNum}C`;
	}

	function formatDateForInput(date: Date): string {
		const year = date.getFullYear();
		const month = String(date.getMonth() + 1).padStart(2, '0');
		const day = String(date.getDate()).padStart(2, '0');
		return `${year}-${month}-${day}`;
	}

	function getRowClass(test: TestCylinderQueueResponse): string {
		const today = new Date();
		today.setHours(0, 0, 0, 0);

		const dateDue = test.dateDue ? new Date(test.dateDue) : null;
		if (!dateDue) return '';

		dateDue.setHours(0, 0, 0, 0);

		// Completed tests (grey/muted)
		if (test.dateTested) {
			return 'completed';
		}

		// Overdue tests (red)
		if (dateDue < today) {
			return 'overdue';
		}

		// Due today (yellow)
		if (dateDue.getTime() === today.getTime()) {
			return 'due-today';
		}

		// Upcoming (normal)
		return 'upcoming';
	}

	function getStatusText(test: TestCylinderQueueResponse): string {
		const today = new Date();
		today.setHours(0, 0, 0, 0);

		const dateDue = test.dateDue ? new Date(test.dateDue) : null;
		if (!dateDue) return '';

		dateDue.setHours(0, 0, 0, 0);

		// Completed tests
		if (test.dateTested) {
			return '✓ Tested';
		}

		// Overdue tests
		if (dateDue < today) {
			const diffMs = today.getTime() - dateDue.getTime();
			const days = Math.floor(diffMs / (1000 * 60 * 60 * 24));
			const hours = Math.floor((diffMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));

			if (days > 0) {
				return hours > 0 ? `${days}d ${hours}h overdue` : `${days}d overdue`;
			} else {
				return `${hours}h overdue`;
			}
		}

		// Due today
		if (dateDue.getTime() === today.getTime()) {
			return 'Due today';
		}

		// Upcoming
		const diffMs = dateDue.getTime() - today.getTime();
		const days = Math.ceil(diffMs / (1000 * 60 * 60 * 24));
		return days === 1 ? 'Due tomorrow' : `Due in ${days} days`;
	}
</script>

<div class="test-queue">
	<div class="header">
		<h2>Test Queue</h2>
		<div class="date-selector">
			<label for="end-date">Show tests through:</label>
			<input
				id="end-date"
				type="date"
				value={formatDateForInput(endDate)}
				onchange={handleEndDateChange}
			/>
		</div>
	</div>

	{#if loading}
		<p>Loading tests...</p>
	{:else if error}
		<p class="error">{error}</p>
	{:else if tests.length === 0}
		<p>No tests in queue</p>
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
					<th>Status</th>
				</tr>
			</thead>
			<tbody>
				{#each tests as test}
					<tr
						class="clickable {getRowClass(test)}"
						onclick={() => handleRowClick(test.testSetDayId)}
					>
						<td>{test.testCylinderCode}</td>
						<td>{formatCylinder(test.dayNum)}</td>
						<td>{test.jobCode} - {test.jobName}</td>
						<td>{test.mixDesignCode}</td>
						<td>{test.requiredPsi}</td>
						<td>{test.pieceType}</td>
						<td>{formatDate(test.castDate)}</td>
						<td class="status-cell">{getStatusText(test)}</td>
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
	.test-queue {
		padding: 1rem;
	}

	.header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 1rem;
	}

	h2 {
		margin: 0;
	}

	.date-selector {
		display: flex;
		align-items: center;
		gap: 0.5rem;
	}

	.date-selector label {
		font-size: 0.9rem;
	}

	.date-selector input[type="date"] {
		padding: 0.25rem 0.5rem;
		border: 1px solid #ccc;
		border-radius: 4px;
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

	tbody tr {
		cursor: pointer;
	}

	tbody tr:hover {
		opacity: 0.8;
	}

	/* Row styling based on status */
	tbody tr.overdue {
		background-color: #ffebee; /* Light red */
	}

	tbody tr.overdue:hover {
		background-color: #ffcdd2; /* Slightly darker red on hover */
	}

	tbody tr.due-today {
		background-color: #fff9c4; /* Light yellow */
	}

	tbody tr.due-today:hover {
		background-color: #fff59d; /* Slightly darker yellow on hover */
	}

	tbody tr.completed {
		background-color: #f5f5f5; /* Light grey */
		opacity: 0.7;
	}

	tbody tr.completed:hover {
		background-color: #eeeeee; /* Slightly darker grey on hover */
		opacity: 0.8;
	}

	tbody tr.upcoming {
		background-color: white;
	}

	tbody tr.upcoming:hover {
		background-color: #e3f2fd; /* Light blue on hover */
	}

	.status-cell {
		font-weight: 500;
	}

	.error {
		color: red;
	}
</style>
