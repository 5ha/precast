import { http, HttpResponse } from 'msw';
import type {
	ITestCylinderQueueResponse,
	IGetTestSetDayDetailsResponse,
	ITestCylinderBreakDto
} from '$lib/api/generated/api-client';

const today = new Date();
const yesterday = new Date(today);
yesterday.setDate(yesterday.getDate() - 1);

// Mock data for Today's Testing Queue
const mockTestsDueToday: ITestCylinderQueueResponse[] = [
	{
		testCylinderCode: 'TEST-9011',
		ovenId: 'Oven1',
		dayNum: 7,
		castDate: new Date(yesterday),
		castTime: '08:00:00',
		jobCode: '25-009',
		jobName: 'Dickinson School',
		mixDesignCode: '2509.1',
		requiredPsi: 6000,
		pieceType: 'Walls',
		testSetId: 1,
		testSetDayId: 101,
		dateDue: new Date(today)
	},
	{
		testCylinderCode: 'TEST-9012.1',
		ovenId: 'Oven2',
		dayNum: 1,
		castDate: new Date(yesterday),
		castTime: '09:30:00',
		jobCode: '25-020',
		jobName: 'Woodbury Plaza',
		mixDesignCode: '824.1',
		requiredPsi: 3500,
		pieceType: 'Tees',
		testSetId: 2,
		testSetDayId: 102,
		dateDue: new Date(today)
	},
	{
		testCylinderCode: 'TEST-9015',
		ovenId: 'Oven1',
		dayNum: 28,
		castDate: new Date(yesterday.getTime() - 27 * 24 * 60 * 60 * 1000),
		castTime: '14:15:00',
		jobCode: '25-015',
		jobName: 'Main Street Bridge',
		mixDesignCode: '622.1',
		requiredPsi: 5000,
		pieceType: 'Beams',
		testSetId: 3,
		testSetDayId: 103,
		dateDue: new Date(today)
	},
	{
		testCylinderCode: 'TEST-9016',
		ovenId: 'Oven3',
		dayNum: 7,
		castDate: new Date(yesterday),
		castTime: '10:45:00',
		jobCode: '25-018',
		jobName: 'City Hall Renovation',
		mixDesignCode: '1205.3',
		requiredPsi: 4500,
		pieceType: 'Columns',
		testSetId: 4,
		testSetDayId: 104,
		dateDue: new Date(today)
	},
	{
		testCylinderCode: 'TEST-9017.2',
		ovenId: 'Oven2',
		dayNum: 1,
		castDate: new Date(yesterday),
		castTime: '11:20:00',
		jobCode: '25-021',
		jobName: 'Harbor Park',
		mixDesignCode: '450.2',
		requiredPsi: 4000,
		pieceType: 'Slabs',
		testSetId: 5,
		testSetDayId: 105,
		dateDue: new Date(today)
	}
];

const mockTestsOverdue: ITestCylinderQueueResponse[] = [
	{
		testCylinderCode: 'TEST-9001',
		ovenId: 'Oven1',
		dayNum: 7,
		castDate: new Date(today.getTime() - 10 * 24 * 60 * 60 * 1000),
		castTime: '08:30:00',
		jobCode: '25-005',
		jobName: 'Overdue Project Alpha',
		mixDesignCode: '300.1',
		requiredPsi: 5500,
		pieceType: 'Walls',
		testSetId: 6,
		dateDue: new Date(today.getTime() - 3 * 24 * 60 * 60 * 1000) // 3 days overdue
	},
	{
		testCylinderCode: 'TEST-9002',
		ovenId: 'Oven2',
		dayNum: 1,
		castDate: new Date(today.getTime() - 2 * 24 * 60 * 60 * 1000),
		castTime: '10:00:00',
		jobCode: '25-007',
		jobName: 'Downtown Plaza',
		mixDesignCode: '500.2',
		requiredPsi: 4000,
		pieceType: 'Slabs',
		testSetId: 7,
		dateDue: new Date(today.getTime() - 1 * 24 * 60 * 60 * 1000) // 1 day overdue
	},
	{
		testCylinderCode: 'TEST-9003',
		ovenId: 'Oven3',
		dayNum: 28,
		castDate: new Date(today.getTime() - 33 * 24 * 60 * 60 * 1000),
		castTime: '13:45:00',
		jobCode: '25-003',
		jobName: 'Industrial Complex',
		mixDesignCode: '1000.5',
		requiredPsi: 7000,
		pieceType: 'Beams',
		testSetId: 8,
		dateDue: new Date(today.getTime() - 5 * 24 * 60 * 60 * 1000) // 5 days overdue
	},
	{
		testCylinderCode: 'TEST-9004',
		ovenId: 'Oven1',
		dayNum: 7,
		castDate: new Date(today.getTime() - 8 * 24 * 60 * 60 * 1000),
		castTime: '07:15:00',
		jobCode: '25-012',
		jobName: 'Warehouse Expansion',
		mixDesignCode: '622.3',
		requiredPsi: 5000,
		pieceType: 'Columns',
		testSetId: 9,
		dateDue: new Date(today.getTime() - 18 * 60 * 60 * 1000) // 18 hours overdue
	}
];

const mockTestsUpcoming: ITestCylinderQueueResponse[] = [
	{
		testCylinderCode: 'TEST-9020',
		ovenId: 'Oven1',
		dayNum: 7,
		castDate: new Date(today.getTime() - 5 * 24 * 60 * 60 * 1000),
		castTime: '09:00:00',
		jobCode: '25-025',
		jobName: 'Future Test Job',
		mixDesignCode: '700.1',
		requiredPsi: 6500,
		pieceType: 'Tees',
		testSetId: 7,
		dateDue: new Date(today.getTime() + 2 * 24 * 60 * 60 * 1000) // 2 days from now
	}
];

// Mock data for test set day details
const mockTestSetDayDetails: Record<number, IGetTestSetDayDetailsResponse> = {
	101: {
		testSetDayId: 101,
		dayNum: 7,
		comments: undefined,
		dateDue: new Date(today),
		dateTested: undefined,
		jobCode: '25-009',
		jobName: 'Dickinson School',
		mixDesignCode: '2509.1',
		requiredPsi: 6000,
		pieceType: 'Walls',
		castDate: new Date(yesterday),
		castTime: '08:00:00',
		testCylinders: [
			{ testCylinderId: 1001, code: 'TEST-9011-1', breakPsi: undefined },
			{ testCylinderId: 1002, code: 'TEST-9011-2', breakPsi: undefined },
			{ testCylinderId: 1003, code: 'TEST-9011-3', breakPsi: undefined }
		]
	},
	102: {
		testSetDayId: 102,
		dayNum: 1,
		comments: undefined,
		dateDue: new Date(today),
		dateTested: undefined,
		jobCode: '25-020',
		jobName: 'Woodbury Plaza',
		mixDesignCode: '824.1',
		requiredPsi: 3500,
		pieceType: 'Tees',
		castDate: new Date(yesterday),
		castTime: '09:30:00',
		testCylinders: [
			{ testCylinderId: 1004, code: 'TEST-9012.1-1', breakPsi: undefined },
			{ testCylinderId: 1005, code: 'TEST-9012.1-2', breakPsi: undefined },
			{ testCylinderId: 1006, code: 'TEST-9012.1-3', breakPsi: undefined }
		]
	},
	103: {
		testSetDayId: 103,
		dayNum: 28,
		comments: 'Previously tested on day 7',
		dateDue: new Date(today),
		dateTested: new Date(yesterday.getTime() - 21 * 24 * 60 * 60 * 1000),
		jobCode: '25-015',
		jobName: 'Main Street Bridge',
		mixDesignCode: '622.1',
		requiredPsi: 5000,
		pieceType: 'Beams',
		castDate: new Date(yesterday.getTime() - 27 * 24 * 60 * 60 * 1000),
		castTime: '14:15:00',
		testCylinders: [
			{ testCylinderId: 1006, code: 'TEST-9015-1', breakPsi: 5200 },
			{ testCylinderId: 1007, code: 'TEST-9015-2', breakPsi: 5400 },
			{ testCylinderId: 1008, code: 'TEST-9015-3', breakPsi: 5100 }
		]
	}
};

export const handlers = [
	// Handle GET /api/tester-report/tests-due-today
	http.get('/api/tester-report/tests-due-today', () => {
		return HttpResponse.json(mockTestsDueToday);
	}),

	// Handle GET /api/tester-report/tests-overdue
	http.get('/api/tester-report/tests-overdue', () => {
		return HttpResponse.json(mockTestsOverdue);
	}),

	// Handle GET /api/tester-report/tests-upcoming
	http.get('/api/tester-report/tests-upcoming', () => {
		return HttpResponse.json(mockTestsUpcoming);
	}),

	// Handle GET /api/tester-report/test-set-day/:testSetDayId
	http.get('/api/tester-report/test-set-day/:testSetDayId', ({ params }) => {
		const testSetDayId = parseInt(params.testSetDayId as string);
		const details = mockTestSetDayDetails[testSetDayId];

		if (!details) {
			return HttpResponse.json({ message: 'Test set day not found' }, { status: 404 });
		}

		return HttpResponse.json(details);
	}),

	// Handle POST /api/tester-report/test-set-day
	http.post('/api/tester-report/test-set-day', async ({ request }) => {
		const body = (await request.json()) as any;
		console.log('Mock: Saving test set day data:', body);

		// Simulate successful save
		return HttpResponse.json(null, { status: 200 });
	})
];
