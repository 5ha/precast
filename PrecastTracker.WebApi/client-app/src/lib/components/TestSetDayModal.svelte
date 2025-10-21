<script lang="ts">
	import {
		ApiClient,
		GetTestSetDayDetailsResponse,
		SaveTestSetDayDataRequest,
		TestCylinderBreakInput,
		TestCylinderQueueResponse,
		SwaggerException
	} from '$lib/api/generated/api-client';

	interface Props {
		testSetDayId: number | null;
		open: boolean;
		onClose: () => void;
		onSuccess?: (updatedItem: TestCylinderQueueResponse) => void;
	}

	let { testSetDayId, open, onClose, onSuccess }: Props = $props();

	let loading = $state(false);
	let saving = $state(false);
	let error = $state<string | null>(null);
	let details = $state<GetTestSetDayDetailsResponse | null>(null);
	let formData = $state({
		dateTested: '',
		comments: '',
		cylinderBreaks: [] as Array<{ testCylinderId: number; breakPsi: string }>
	});

	// Convert Date to datetime-local input format (YYYY-MM-DDTHH:mm)
	function formatDateTimeForInput(date: Date): string {
		const year = date.getFullYear();
		const month = String(date.getMonth() + 1).padStart(2, '0');
		const day = String(date.getDate()).padStart(2, '0');
		const hours = String(date.getHours()).padStart(2, '0');
		const minutes = String(date.getMinutes()).padStart(2, '0');
		return `${year}-${month}-${day}T${hours}:${minutes}`;
	}

	// Format Date for display with time
	function formatDateTime(date: Date | undefined): string {
		if (!date) return '';
		return new Date(date).toLocaleString();
	}

	// Format Date for display (date only)
	function formatDate(date: Date | undefined): string {
		if (!date) return '';
		return new Date(date).toLocaleDateString();
	}

	// Format TimeSpan for display
	function formatTime(timeSpan: string | undefined): string {
		if (!timeSpan) return '';
		// TimeSpan comes as "HH:mm:ss" format
		return timeSpan.substring(0, 5); // Return just HH:mm
	}

	async function loadTestSetDayDetails() {
		if (!testSetDayId) return;

		loading = true;
		error = null;
		details = null;

		try {
			const client = new ApiClient();
			details = await client.testSetDayGET(testSetDayId);

			// Initialize form data from loaded details
			formData = {
				dateTested: details.dateTested
					? formatDateTimeForInput(new Date(details.dateTested))
					: formatDateTimeForInput(new Date()),
				comments: details.comments || '',
				cylinderBreaks:
					details.testCylinders?.map((c) => ({
						testCylinderId: c.testCylinderId!,
						breakPsi: c.breakPsi?.toString() || ''
					})) || []
			};
		} catch (e) {
			if (e instanceof SwaggerException) {
				error = `Error loading test details: ${e.message}`;
			} else {
				error = 'An unexpected error occurred while loading test details';
			}
		} finally {
			loading = false;
		}
	}

	async function handleSubmit(event: Event) {
		event.preventDefault();
		if (!testSetDayId) return;

		saving = true;
		error = null;

		try {
			const client = new ApiClient();
			const request = new SaveTestSetDayDataRequest({
				testSetDayId: testSetDayId,
				dateTested: new Date(formData.dateTested),
				comments: formData.comments || undefined,
				cylinderBreaks: formData.cylinderBreaks
					.filter((cb) => cb.breakPsi && cb.breakPsi.toString().trim() !== '')
					.map(
						(cb) =>
							new TestCylinderBreakInput({
								testCylinderId: cb.testCylinderId,
								breakPsi: parseInt(cb.breakPsi.toString())
							})
					)
			});

			const updatedItem = await client.testSetDayPOST(request);

			// Success - close modal and pass updated item to callback
			onClose();
			if (onSuccess) {
				onSuccess(updatedItem);
			}
		} catch (e) {
			console.error('Save error:', e);
			if (e instanceof SwaggerException) {
				error = `Error saving test data: ${e.message}`;
			} else if (e instanceof Error) {
				error = `An unexpected error occurred while saving: ${e.message}`;
			} else {
				error = 'An unexpected error occurred while saving';
			}
		} finally {
			saving = false;
		}
	}

	function handleBackdropClick(event: MouseEvent) {
		if (event.target === event.currentTarget) {
			onClose();
		}
	}

	function handleKeyDown(event: KeyboardEvent) {
		if (event.key === 'Escape') {
			onClose();
		}
	}

	// Load details when modal opens or testSetDayId changes
	$effect(() => {
		if (open && testSetDayId) {
			loadTestSetDayDetails();
		}
	});
</script>

<svelte:window on:keydown={handleKeyDown} />

{#if open}
	<!-- svelte-ignore a11y_click_events_have_key_events -->
	<!-- svelte-ignore a11y_no_noninteractive_element_interactions -->
	<div class="modal-overlay" onclick={handleBackdropClick} role="dialog" aria-modal="true" tabindex="-1">
		<div class="modal-content">
			<header>
				<h2>Test Data Entry{details ? ` - ${details.dayNum}C` : ''}</h2>
				<button type="button" class="close-button" onclick={onClose} aria-label="Close">
					×
				</button>
			</header>

			{#if loading}
				<p>Loading test details...</p>
			{:else if error}
				<p class="error">{error}</p>
			{:else if details}
				<div class="test-info">
					<h3>Test Information</h3>
					<div class="info-item">
						<strong>Job:</strong> {details.jobCode} - {details.jobName}
					</div>
					<div class="info-item">
						<strong>Mix Design:</strong> {details.mixDesignCode} ({details.requiredPsi} PSI)
					</div>
					<div class="info-item">
						<strong>Piece Type:</strong> {details.pieceType}
					</div>
					<div class="info-item">
						<strong>Cast Date:</strong>
						{formatDate(details.castDate)}
						{details.castTime ? `at ${formatTime(details.castTime)}` : ''}
					</div>
					<div class="info-item">
						<strong>Due Date:</strong> {formatDate(details.dateDue)}
					</div>
					{#if details.dateTested}
						<div class="info-item">
							<strong>Previously Tested:</strong> {formatDateTime(details.dateTested)}
						</div>
					{/if}
				</div>

				<form onsubmit={handleSubmit}>
					<label for="dateTested">
						Date & Time Tested *
						<input
							id="dateTested"
							type="datetime-local"
							bind:value={formData.dateTested}
							required
						/>
					</label>

					<label for="comments">
						Comments
						<textarea id="comments" bind:value={formData.comments} rows="3"></textarea>
					</label>

					<h3>Test Cylinder Results</h3>
					<table>
						<thead>
							<tr>
								<th>Cylinder Code</th>
								<th>Break PSI</th>
								<th>Status</th>
							</tr>
						</thead>
						<tbody>
							{#each formData.cylinderBreaks as cylinder, i}
								{@const cylinderInfo = details.testCylinders?.[i]}
								{@const breakValue = parseInt(cylinder.breakPsi)}
								{@const isPassing = !isNaN(breakValue) && breakValue >= (details.requiredPsi || 0)}
								<tr>
									<td>{cylinderInfo?.code}</td>
									<td>
										<input
											type="number"
											bind:value={cylinder.breakPsi}
											min="0"
											step="1"
											placeholder="PSI"
										/>
									</td>
									<td>
										{#if cylinder.breakPsi && !isNaN(breakValue)}
											{isPassing ? 'Pass' : 'Fail'}
										{/if}
									</td>
								</tr>
							{/each}
						</tbody>
					</table>

					<div>
						<button type="button" onclick={onClose}>Cancel</button>
						<button type="submit" disabled={saving}>
							{saving ? 'Saving...' : 'Save Test Data'}
						</button>
					</div>
				</form>
			{/if}
		</div>
	</div>
{/if}

<style>
	.modal-overlay {
		position: fixed;
		top: 0;
		left: 0;
		right: 0;
		bottom: 0;
		background-color: rgba(0, 0, 0, 0.5);
		display: flex;
		align-items: center;
		justify-content: center;
		z-index: 1000;
	}

	.modal-content {
		background: white;
		border: 1px solid #ddd;
		max-width: 800px;
		width: 90%;
		max-height: 90vh;
		overflow-y: auto;
		padding: 1rem;
	}

	header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 1rem;
	}

	h2 {
		margin: 0;
	}

	h3 {
		margin-bottom: 0.5rem;
	}

	.close-button {
		background: none;
		border: none;
		font-size: 1.5rem;
		cursor: pointer;
	}

	.test-info {
		margin-bottom: 1rem;
		padding: 0.5rem;
		background-color: #f9f9f9;
		border: 1px solid #ddd;
	}

	.info-item {
		margin-bottom: 0.25rem;
	}

	label {
		display: block;
		margin-bottom: 1rem;
	}

	input,
	textarea {
		width: 100%;
		padding: 0.5rem;
		border: 1px solid #ddd;
	}

	table {
		width: 100%;
		border-collapse: collapse;
		margin-bottom: 1rem;
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

	button {
		padding: 0.5rem 1rem;
		margin-left: 0.5rem;
		cursor: pointer;
	}

	.error {
		color: red;
	}
</style>
