# Minimum Safe Release Unit Test Checklist

- Scope: minimum-safe-release unit tests for AISEP backend
- Total tests listed: 278
- Expected target range: 220-280 tests
- Naming convention: xUnit style test method names

## How to use
1. Keep test name unchanged when implementing to make review easier.
2. Mark each checkbox when test is implemented and passing.
3. Prioritize sections 1-8 before release candidate.

## 1) AuthService (37 tests)

### Function: RegisterAsync (5 tests)
- [x] UT001 - AuthService_RegisterAsync_ShouldFail_WhenEmailAlreadyRegistered
- [x] UT002 - AuthService_RegisterAsync_ShouldFail_WhenRoleIsAdminOrStaff
- [x] UT003 - AuthService_RegisterAsync_ShouldFail_WhenCreateUserReturnsErrors
- [x] UT004 - AuthService_RegisterAsync_ShouldSendEmailConfirmation_WhenRegistrationSucceeds
- [x] UT005 - AuthService_RegisterAsync_ShouldReturnUserIdAndEmail_WhenRegistrationSucceeds

### Function: ConfirmEmailAsync (4 tests)
- [x] UT006 - AuthService_ConfirmEmailAsync_ShouldFail_WhenUserNotFound
- [x] UT007 - AuthService_ConfirmEmailAsync_ShouldReturnAlreadyConfirmed_WhenEmailAlreadyConfirmed
- [x] UT008 - AuthService_ConfirmEmailAsync_ShouldFail_WhenTokenInvalid
- [x] UT009 - AuthService_ConfirmEmailAsync_ShouldSetUserActive_WhenConfirmationSucceeds

### Function: ResendConfirmationAsync (4 tests)
- [x] UT010 - AuthService_ResendConfirmationAsync_ShouldReturnGenericSuccess_WhenUserNotFound
- [x] UT011 - AuthService_ResendConfirmationAsync_ShouldFail_WhenEmailAlreadyConfirmed
- [x] UT012 - AuthService_ResendConfirmationAsync_ShouldFail_WhenEmailSendingThrows
- [x] UT013 - AuthService_ResendConfirmationAsync_ShouldSucceed_WhenEmailSent

### Function: LoginAsync (7 tests)
- [x] UT014 - AuthService_LoginAsync_ShouldFail_WhenUserNotFound
- [x] UT015 - AuthService_LoginAsync_ShouldFail_WhenUserIsBanned
- [x] UT016 - AuthService_LoginAsync_ShouldFail_WhenEmailNotConfirmed
- [x] UT017 - AuthService_LoginAsync_ShouldFail_WhenAccountIsLockedOut
- [x] UT018 - AuthService_LoginAsync_ShouldFail_WhenPasswordInvalid
- [x] UT019 - AuthService_LoginAsync_ShouldReturnTokens_WhenCredentialsValid
- [x] UT020 - AuthService_LoginAsync_ShouldPersistRefreshToken_WhenCredentialsValid

### Function: RefreshTokenAsync (4 tests)
- [ ] UT021 - AuthService_RefreshTokenAsync_ShouldFail_WhenTokenNotFound
- [ ] UT022 - AuthService_RefreshTokenAsync_ShouldFail_WhenUserNotFound
- [ ] UT023 - AuthService_RefreshTokenAsync_ShouldFail_WhenTokenInactive
- [ ] UT024 - AuthService_RefreshTokenAsync_ShouldRevokeOldTokenAndCreateNewToken_WhenTokenValid

### Function: RevokeTokenAsync (3 tests)
- [ ] UT025 - AuthService_RevokeTokenAsync_ShouldFail_WhenTokenNotFound
- [ ] UT026 - AuthService_RevokeTokenAsync_ShouldFail_WhenTokenAlreadyInactive
- [ ] UT027 - AuthService_RevokeTokenAsync_ShouldRevokeToken_WhenTokenActive

### Function: LogoutAsync (1 tests)
- [x] UT028 - AuthService_LogoutAsync_ShouldRevokeAllActiveTokensAndSignOut

### Function: ForgotPasswordAsync (3 tests)
- [x] UT029 - AuthService_ForgotPasswordAsync_ShouldReturnGenericSuccess_WhenEmailEmpty
- [x] UT030 - AuthService_ForgotPasswordAsync_ShouldFail_WhenEmailSendingThrows
- [x] UT031 - AuthService_ForgotPasswordAsync_ShouldSendResetEmail_WhenUserExists

### Function: ResetPasswordAsync (3 tests)
- [x] UT032 - AuthService_ResetPasswordAsync_ShouldFail_WhenUserNotFound
- [x] UT033 - AuthService_ResetPasswordAsync_ShouldFallbackToRawToken_WhenBase64DecodeFails
- [x] UT034 - AuthService_ResetPasswordAsync_ShouldRevokeAllActiveTokens_WhenResetSucceeds

### Function: ChangePasswordAsync (3 tests)
- [x] UT035 - AuthService_ChangePasswordAsync_ShouldFail_WhenUserNotFound
- [x] UT036 - AuthService_ChangePasswordAsync_ShouldReturnErrors_WhenIdentityChangeFails
- [x] UT037 - AuthService_ChangePasswordAsync_ShouldRevokeAllActiveTokens_WhenChangeSucceeds

## 2) PaymentService (36 tests)

### Function: GetInvestorPackagesAsync (1 tests)
- [x] UT038 - PaymentService_GetInvestorPackagesAsync_ShouldReturnOnlyInvestorPackages

### Function: GetStartupPackagesAsync (1 tests)
- [x] UT039 - PaymentService_GetStartupPackagesAsync_ShouldReturnOnlyStartupPackages

### Function: CheckoutSubscriptionAsync (8 tests)
- [x] UT040 - PaymentService_CheckoutSubscriptionAsync_ShouldThrow_WhenPackageIdIsNotPositive
- [x] UT041 - PaymentService_CheckoutSubscriptionAsync_ShouldThrow_WhenPackageNotFound
- [x] UT042 - PaymentService_CheckoutSubscriptionAsync_ShouldThrow_WhenUserNotFound
- [x] UT043 - PaymentService_CheckoutSubscriptionAsync_ShouldThrow_WhenPackageRoleMismatch
- [x] UT044 - PaymentService_CheckoutSubscriptionAsync_ShouldReusePendingTransaction_WhenPendingNotExpired
- [x] UT045 - PaymentService_CheckoutSubscriptionAsync_ShouldFailOldPendingAndCreateNew_WhenPendingExpired
- [x] UT046 - PaymentService_CheckoutSubscriptionAsync_ShouldGeneratePaymentCodeWithPrefix_WhenCreated
- [x] UT047 - PaymentService_CheckoutSubscriptionAsync_ShouldReturnQrCodeUrl_WhenCreated

### Function: CheckoutBookingAsync (4 tests)
- [x] UT048 - PaymentService_CheckoutBookingAsync_ShouldThrow_WhenBookingIdIsNotPositive
- [x] UT049 - PaymentService_CheckoutBookingAsync_ShouldThrow_WhenPayableBookingNotFound
- [x] UT050 - PaymentService_CheckoutBookingAsync_ShouldReusePendingTransaction_WhenPendingNotExpired
- [x] UT051 - PaymentService_CheckoutBookingAsync_ShouldCreatePendingTransaction_WhenNoPendingExists

### Function: UpdatePackageAsync (7 tests)
- [x] UT052 - PaymentService_UpdatePackageAsync_ShouldThrow_WhenPackageIdIsNotPositive
- [x] UT053 - PaymentService_UpdatePackageAsync_ShouldThrow_WhenPackageNotFound
- [x] UT054 - PaymentService_UpdatePackageAsync_ShouldThrow_WhenPackageTargetRoleNotSupported
- [x] UT055 - PaymentService_UpdatePackageAsync_ShouldThrow_WhenPriceIsNotPositive
- [x] UT056 - PaymentService_UpdatePackageAsync_ShouldThrow_WhenDurationMonthsIsNotPositive
- [x] UT057 - PaymentService_UpdatePackageAsync_ShouldThrow_WhenPackageNameIsEmpty
- [x] UT058 - PaymentService_UpdatePackageAsync_ShouldPersistFields_WhenInputValid

### Function: GetTransactionStatusAsync (3 tests)
- [ ] UT059 - PaymentService_GetTransactionStatusAsync_ShouldThrow_WhenTransactionNotFound
- [ ] UT060 - PaymentService_GetTransactionStatusAsync_ShouldAutoFailPending_WhenExpired
- [ ] UT061 - PaymentService_GetTransactionStatusAsync_ShouldReturnMappedStatus_WhenFound

### Function: GetBookingPaymentStatusAsync (3 tests)
- [ ] UT062 - PaymentService_GetBookingPaymentStatusAsync_ShouldThrow_WhenBookingNotFound
- [ ] UT063 - PaymentService_GetBookingPaymentStatusAsync_ShouldThrow_WhenRequesterIsNotCustomer
- [ ] UT064 - PaymentService_GetBookingPaymentStatusAsync_ShouldAutoFailLatestPending_WhenExpired

### Function: GetBookingPaymentTransactionsAsync (1 tests)
- [ ] UT065 - PaymentService_GetBookingPaymentTransactionsAsync_ShouldReturnOnlyBookingReferenceTransactions

### Function: ProcessSePayWebhookAsync (8 tests)
- [ ] UT066 - PaymentService_ProcessSePayWebhookAsync_ShouldThrow_WhenPaymentCodeNotFoundInPayload
- [ ] UT067 - PaymentService_ProcessSePayWebhookAsync_ShouldReturnIdempotent_WhenTransactionAlreadyCompleted
- [ ] UT068 - PaymentService_ProcessSePayWebhookAsync_ShouldThrow_WhenPendingTransactionNotFound
- [ ] UT069 - PaymentService_ProcessSePayWebhookAsync_ShouldThrow_WhenTransferAmountInsufficient
- [ ] UT070 - PaymentService_ProcessSePayWebhookAsync_ShouldActivateSubscription_WhenReferenceTypeSubscription
- [ ] UT071 - PaymentService_ProcessSePayWebhookAsync_ShouldSetUserPremium_WhenSubscriptionActivated
- [ ] UT072 - PaymentService_ProcessSePayWebhookAsync_ShouldConfirmBookingAndNotifyBothSides_WhenReferenceTypeBooking
- [ ] UT073 - PaymentService_ProcessSePayWebhookAsync_ShouldPersistSePayFieldsAndCompletedAt_WhenProcessed

## 3) DealService (34 tests)

### Function: CreateDealAsync (6 tests)
- [x] UT074 - DealService_CreateDealAsync_ShouldThrow_WhenProjectIdIsNotPositive
- [x] UT075 - DealService_CreateDealAsync_ShouldThrow_WhenInvestorNotFound
- [x] UT076 - DealService_CreateDealAsync_ShouldThrow_WhenProjectNotFound
- [x] UT077 - DealService_CreateDealAsync_ShouldThrow_WhenBlockingDealExists
- [x] UT078 - DealService_CreateDealAsync_ShouldSetPendingFlags_WhenCreated
- [x] UT079 - DealService_CreateDealAsync_ShouldSendNotificationToStartup_WhenCreated

### Function: GetDealsAsync (1 tests)
- [x] UT080 - DealService_GetDealsAsync_ShouldReturnPagedDeals

### Function: GetInvestorDealsAsync (1 tests)
- [x] UT081 - DealService_GetInvestorDealsAsync_ShouldFilterByInvestorId

### Function: GetStartupDealsAsync (1 tests)
- [x] UT082 - DealService_GetStartupDealsAsync_ShouldFilterByStartupId

### Function: RespondDealAsync (6 tests)
- [x] UT083 - DealService_RespondDealAsync_ShouldThrow_WhenDealNotFound
- [x] UT084 - DealService_RespondDealAsync_ShouldThrowForbidden_WhenStartupDoesNotOwnProject
- [x] UT085 - DealService_RespondDealAsync_ShouldThrow_WhenDealStatusIsNotPending
- [x] UT086 - DealService_RespondDealAsync_ShouldSetConfirmed_WhenAccepted
- [x] UT087 - DealService_RespondDealAsync_ShouldSetRejected_WhenRejected
- [x] UT088 - DealService_RespondDealAsync_ShouldNotifyInvestor_WhenResponded

### Function: GetContractPreviewForInvestorAsync (3 tests)
- [x] UT089 - DealService_GetContractPreviewForInvestorAsync_ShouldThrowForbidden_WhenInvestorDoesNotOwnDeal
- [x] UT090 - DealService_GetContractPreviewForInvestorAsync_ShouldThrow_WhenStatusNotInSigningFlow
- [x] UT091 - DealService_GetContractPreviewForInvestorAsync_ShouldReturnHtml_WhenValid

### Function: InvestorSignContractAsync (4 tests)
- [x] UT092 - DealService_InvestorSignContractAsync_ShouldThrow_WhenFinalAmountIsNotPositive
- [x] UT093 - DealService_InvestorSignContractAsync_ShouldThrow_WhenFinalEquityPercentageIsNegative
- [x] UT094 - DealService_InvestorSignContractAsync_ShouldThrow_WhenDealStatusIsNotConfirmed
- [x] UT095 - DealService_InvestorSignContractAsync_ShouldSetWaitingForStartupSignature_WhenSuccessful

### Function: StartupSignContractAsync (6 tests)
- [x] UT096 - DealService_StartupSignContractAsync_ShouldThrow_WhenDealStatusIsNotWaitingForStartupSignature
- [x] UT097 - DealService_StartupSignContractAsync_ShouldThrow_WhenInvestorSignatureMissing
- [x] UT098 - DealService_StartupSignContractAsync_ShouldThrow_WhenPdfGenerationFails
- [x] UT099 - DealService_StartupSignContractAsync_ShouldThrow_WhenPdfUploadFails
- [x] UT100 - DealService_StartupSignContractAsync_ShouldFallbackDirectBlockchainCall_WhenQueueFails
- [x] UT101 - DealService_StartupSignContractAsync_ShouldSetContractSignedAndNotifyInvestor_WhenSuccessful

### Function: StartupRejectContractAsync (2 tests)
- [x] UT102 - DealService_StartupRejectContractAsync_ShouldThrow_WhenDealStatusIsNotWaitingForStartupSignature
- [x] UT103 - DealService_StartupRejectContractAsync_ShouldSetRejectedAndClearSignatures_WhenSuccessful

### Function: GetContractStatusForInvestorAsync (1 tests)
- [x] UT104 - DealService_GetContractStatusForInvestorAsync_ShouldThrowForbidden_WhenInvestorDoesNotOwnDeal

### Function: GetOwnershipAssignmentStatusAsync (3 tests)
- [ ] UT105 - DealService_GetOwnershipAssignmentStatusAsync_ShouldThrow_WhenNoRegisteredProjectDocumentExists
- [ ] UT106 - DealService_GetOwnershipAssignmentStatusAsync_ShouldReturnAssigned_WhenInvestorWalletExistsOnChain
- [ ] UT107 - DealService_GetOwnershipAssignmentStatusAsync_ShouldReturnUnassigned_WhenInvestorWalletMissingOnChain

## 4) BookingService (22 tests)

### Function: CreateBookingAsync (15 tests)
- [x] UT108 - BookingService_CreateBookingAsync_ShouldThrow_WhenNoSlotSelected
- [x] UT109 - BookingService_CreateBookingAsync_ShouldThrow_WhenAdvisorNotFound
- [x] UT110 - BookingService_CreateBookingAsync_ShouldThrow_WhenProjectNotFound
- [x] UT111 - BookingService_CreateBookingAsync_ShouldThrow_WhenProjectHasNoAdvisorAssignment
- [x] UT112 - BookingService_CreateBookingAsync_ShouldThrow_WhenSelectedSlotIdsContainMissingItems
- [x] UT113 - BookingService_CreateBookingAsync_ShouldThrow_WhenSelectedSlotsBelongToDifferentAdvisor
- [x] UT114 - BookingService_CreateBookingAsync_ShouldThrow_WhenAnySelectedSlotNotAvailable
- [x] UT115 - BookingService_CreateBookingAsync_ShouldThrow_WhenAnySelectedSlotInPast
- [x] UT116 - BookingService_CreateBookingAsync_ShouldThrow_WhenSelectedSlotsAreNotConsecutive
- [x] UT117 - BookingService_CreateBookingAsync_ShouldThrow_WhenBookingLessThan12HoursInAdvance
- [x] UT118 - BookingService_CreateBookingAsync_ShouldThrow_WhenFreeRebookFromComplaintAlreadyUsed
- [x] UT119 - BookingService_CreateBookingAsync_ShouldThrow_WhenPremiumFreeBookingDurationExceeds3Hours
- [x] UT120 - BookingService_CreateBookingAsync_ShouldThrow_WhenPremiumFreeQuotaNotAvailable
- [x] UT121 - BookingService_CreateBookingAsync_ShouldCreateBookingAndReserveSlots_WhenValid
- [x] UT122 - BookingService_CreateBookingAsync_ShouldNotifyAdvisor_WhenBookingCreated

### Function: ApproveBookingAsync (4 tests)
- [x] UT123 - BookingService_ApproveBookingAsync_ShouldThrow_WhenBookingNotFound
- [x] UT124 - BookingService_ApproveBookingAsync_ShouldThrow_WhenAdvisorResponseWindowExpired
- [x] UT125 - BookingService_ApproveBookingAsync_ShouldSetConfirmed_WhenPaymentWaivedOrPriceZero
- [x] UT126 - BookingService_ApproveBookingAsync_ShouldSetApprovedAwaitingPayment_WhenPaymentRequired

### Function: RejectBookingAsync (2 tests)
- [x] UT127 - BookingService_RejectBookingAsync_ShouldThrow_WhenStatusCannotBeRejected
- [x] UT128 - BookingService_RejectBookingAsync_ShouldReleaseSlotsAndNotifyCustomer_WhenRejected

### Function: ExpirePendingAdvisorResponsesAsync (1 tests)
- [ ] UT129 - BookingService_ExpirePendingAdvisorResponsesAsync_ShouldMarkNoResponseAndNotifyCustomer

## 5) ConsultingReportService (19 tests)

### Function: CreateAsync (8 tests)
- [x] UT130 - ConsultingReportService_CreateAsync_ShouldThrowForbidden_WhenCurrentUserIsNotAdvisor
- [x] UT131 - ConsultingReportService_CreateAsync_ShouldThrow_WhenBookingNotFound
- [x] UT132 - ConsultingReportService_CreateAsync_ShouldThrowForbidden_WhenAdvisorNotAssignedToBooking
- [x] UT133 - ConsultingReportService_CreateAsync_ShouldThrow_WhenBookingStatusIsNotConfirmed
- [x] UT134 - ConsultingReportService_CreateAsync_ShouldThrow_WhenSubmissionWindowExpired
- [x] UT135 - ConsultingReportService_CreateAsync_ShouldCreateSubmittedReport_WhenNoExistingReport
- [x] UT136 - ConsultingReportService_CreateAsync_ShouldThrow_WhenExistingReportIsNotRevisionRequested
- [x] UT137 - ConsultingReportService_CreateAsync_ShouldUpdateReport_WhenRevisionRequestedAndWithinDeadline

### Function: ApproveAsync (3 tests)
- [x] UT138 - ConsultingReportService_ApproveAsync_ShouldThrow_WhenReportStatusIsNotSubmitted
- [x] UT139 - ConsultingReportService_ApproveAsync_ShouldCompleteBookingAndCloseChat_WhenApproved
- [x] UT140 - ConsultingReportService_ApproveAsync_ShouldDisburseAdvisorPayout_WhenApproved

### Function: RequestRevisionAsync (3 tests)
- [x] UT141 - ConsultingReportService_RequestRevisionAsync_ShouldThrow_WhenReportStatusIsNotSubmitted
- [x] UT142 - ConsultingReportService_RequestRevisionAsync_ShouldEscalateToStaff_WhenRevisionCountReachedMax
- [x] UT143 - ConsultingReportService_RequestRevisionAsync_ShouldSetRevisionRequestedAndAdvisorDue_WhenUnderLimit

### Function: AcceptComplaintByStaffAsync (2 tests)
- [ ] UT144 - ConsultingReportService_AcceptComplaintByStaffAsync_ShouldThrow_WhenReportStatusIsNotEscalated
- [ ] UT145 - ConsultingReportService_AcceptComplaintByStaffAsync_ShouldSkipPayoutAndRefundQuota_WhenAccepted

### Function: RejectComplaintByStaffAsync (1 tests)
- [ ] UT146 - ConsultingReportService_RejectComplaintByStaffAsync_ShouldDisbursePayout_WhenRejected

### Function: ProcessReportDeadlinesAsync (2 tests)
- [x] UT147 - ConsultingReportService_ProcessReportDeadlinesAsync_ShouldAutoApprove_WhenStartupReviewTimesOut
- [x] UT148 - ConsultingReportService_ProcessReportDeadlinesAsync_ShouldEscalateToStaff_WhenAdvisorRevisionTimesOut

## 6) ProjectService (15 tests)

### Function: GetProjectByIdAsync (6 tests)
- [x] UT149 - ProjectService_GetProjectByIdAsync_ShouldThrow_WhenProjectNotFound
- [x] UT150 - ProjectService_GetProjectByIdAsync_ShouldBypassQuota_WhenRoleIsStaffOrAdmin
- [x] UT151 - ProjectService_GetProjectByIdAsync_ShouldBypassQuota_WhenStartupOwnsProject
- [x] UT152 - ProjectService_GetProjectByIdAsync_ShouldThrow_WhenNoActiveSubscriptionForQuotaRoles
- [x] UT153 - ProjectService_GetProjectByIdAsync_ShouldThrow_WhenProjectViewQuotaExceeded
- [x] UT154 - ProjectService_GetProjectByIdAsync_ShouldConsumeQuotaAndUnlockProject_WhenFirstView

### Function: CreateProjectAsync (2 tests)
- [x] UT155 - ProjectService_CreateProjectAsync_ShouldThrow_WhenStartupProfileNotFound
- [x] UT156 - ProjectService_CreateProjectAsync_ShouldSetDraftAndUploadImage_WhenValidRequest

### Function: UpdateProjectAsync (4 tests)
- [x] UT157 - ProjectService_UpdateProjectAsync_ShouldThrow_WhenProjectNotFound
- [x] UT158 - ProjectService_UpdateProjectAsync_ShouldThrowForbidden_WhenStartupDoesNotOwnProject
- [x] UT159 - ProjectService_UpdateProjectAsync_ShouldThrow_WhenStatusIsNotDraftOrRejected
- [x] UT160 - ProjectService_UpdateProjectAsync_ShouldMoveRejectedToDraft_BeforeApplyingUpdates

### Function: SubmitProjectAsync (1 tests)
- [x] UT161 - ProjectService_SubmitProjectAsync_ShouldThrow_WhenProjectStatusIsNotDraft

### Function: RejectProjectAsync (2 tests)
- [x] UT162 - ProjectService_RejectProjectAsync_ShouldThrow_WhenProjectStatusIsNotPending
- [x] UT163 - ProjectService_RejectProjectAsync_ShouldSetRejectedMetadata_WhenSuccessful

## 7) DocumentService (18 tests)

### Function: UploadDocumentAsync (7 tests)
- [x] UT164 - DocumentService_UploadDocumentAsync_ShouldThrow_WhenProjectNotFound
- [x] UT165 - DocumentService_UploadDocumentAsync_ShouldThrowUnauthorized_WhenStartupDoesNotOwnProject
- [x] UT166 - DocumentService_UploadDocumentAsync_ShouldThrow_WhenProjectStatusIsNotDraft
- [x] UT167 - DocumentService_UploadDocumentAsync_ShouldThrow_WhenDuplicateFileHashExistsInDatabase
- [x] UT168 - DocumentService_UploadDocumentAsync_ShouldThrow_WhenFileHashAlreadyExistsOnBlockchain
- [x] UT169 - DocumentService_UploadDocumentAsync_ShouldWrapError_WhenBlockchainVerifyFailsUnexpectedly
- [x] UT170 - DocumentService_UploadDocumentAsync_ShouldPersistDocumentWithHashAndUrl_WhenValid

### Function: GetByIdAsync (2 tests)
- [x] UT171 - DocumentService_GetByIdAsync_ShouldReturnNull_WhenDocumentNotFound
- [x] UT172 - DocumentService_GetByIdAsync_ShouldThrowUnauthorized_WhenUserCannotViewProjectDocuments

### Function: DeleteAsync (4 tests)
- [x] UT173 - DocumentService_DeleteAsync_ShouldReturnFalse_WhenDocumentNotFound
- [x] UT174 - DocumentService_DeleteAsync_ShouldThrowUnauthorized_WhenStartupCannotDeleteForeignDocument
- [x] UT175 - DocumentService_DeleteAsync_ShouldThrow_WhenProjectIsLockedByApprovedStatus
- [x] UT176 - DocumentService_DeleteAsync_ShouldDeleteDocument_WhenAllowed

### Function: VerifyDocumentAsync (2 tests)
- [x] UT177 - DocumentService_VerifyDocumentAsync_ShouldThrow_WhenDocumentNotFound
- [x] UT178 - DocumentService_VerifyDocumentAsync_ShouldThrow_WhenDocumentNotRegisteredOnBlockchain

### Function: ApproveProjectAsync (3 tests)
- [x] UT179 - DocumentService_ApproveProjectAsync_ShouldThrow_WhenProjectStatusIsNotPending
- [x] UT180 - DocumentService_ApproveProjectAsync_ShouldThrow_WhenNoProjectDocumentExists
- [x] UT181 - DocumentService_ApproveProjectAsync_ShouldRegisterHashAndApproveProject_WhenValid

## 8) ConnectionService (14 tests)

### Function: GetInvestorRequestsAsync (2 tests)
- [x] UT182 - ConnectionService_GetInvestorRequestsAsync_ShouldThrow_WhenStatusFilterInvalid
- [x] UT183 - ConnectionService_GetInvestorRequestsAsync_ShouldApplyStatusFilter_WhenProvided

### Function: GetStartupRequestsAsync (1 tests)
- [x] UT184 - ConnectionService_GetStartupRequestsAsync_ShouldApplyStatusFilter_WhenProvided

### Function: CreateRequestAsync (4 tests)
- [x] UT185 - ConnectionService_CreateRequestAsync_ShouldThrow_WhenInvestorNotFound
- [x] UT186 - ConnectionService_CreateRequestAsync_ShouldThrow_WhenProjectNotFound
- [x] UT187 - ConnectionService_CreateRequestAsync_ShouldThrow_WhenPendingRequestAlreadyExists
- [x] UT188 - ConnectionService_CreateRequestAsync_ShouldPersistPendingRequestAndNotifyStartup_WhenValid

### Function: RespondToRequestAsync (6 tests)
- [x] UT189 - ConnectionService_RespondToRequestAsync_ShouldThrow_WhenRequestNotFound
- [x] UT190 - ConnectionService_RespondToRequestAsync_ShouldThrowForbidden_WhenStartupDoesNotOwnProject
- [x] UT191 - ConnectionService_RespondToRequestAsync_ShouldThrow_WhenRequestStatusIsNotPending
- [x] UT192 - ConnectionService_RespondToRequestAsync_ShouldOpenChatSession_WhenAccepted
- [x] UT193 - ConnectionService_RespondToRequestAsync_ShouldThrow_WhenAcceptedButChatSessionNotCreated
- [x] UT194 - ConnectionService_RespondToRequestAsync_ShouldNotifyInvestorWithConnectionReference_WhenRejected

### Function: GetFounderContactAsync (1 tests)
- [x] UT195 - ConnectionService_GetFounderContactAsync_ShouldThrowUnauthorized_WhenNoAcceptedConnectionExists

## 9) ChatSessionService (10 tests)

### Function: OpenSessionAsync (5 tests)
- [x] UT196 - ChatSessionService_OpenSessionAsync_ShouldReturnNull_WhenBookingNotFound
- [x] UT197 - ChatSessionService_OpenSessionAsync_ShouldReturnNull_WhenUserNotBookingParticipant
- [x] UT198 - ChatSessionService_OpenSessionAsync_ShouldReturnNull_WhenBookingStatusIsNotConfirmed
- [x] UT199 - ChatSessionService_OpenSessionAsync_ShouldReturnExistingSession_WhenAlreadyExists
- [x] UT200 - ChatSessionService_OpenSessionAsync_ShouldCreateSession_WhenNotExistsAndConfirmed

### Function: OpenSessionByConnectionRequestAsync (2 tests)
- [x] UT201 - ChatSessionService_OpenSessionByConnectionRequestAsync_ShouldReturnNull_WhenRequestInvalidOrNotAccepted
- [x] UT202 - ChatSessionService_OpenSessionByConnectionRequestAsync_ShouldReturnExistingSession_WhenAlreadyExists

### Function: GetSessionAsync (1 tests)
- [x] UT203 - ChatSessionService_GetSessionAsync_ShouldReturnNull_WhenUserNotParticipant

### Function: CloseSessionAsync (2 tests)
- [x] UT204 - ChatSessionService_CloseSessionAsync_ShouldReturnFalse_WhenSessionMissingClosedOrUnauthorized
- [x] UT205 - ChatSessionService_CloseSessionAsync_ShouldCloseSessionAndSetEndTime_WhenAuthorized

## 10) ChatMessageService (8 tests)

### Function: GetMessagesAsync (3 tests)
- [x] UT206 - ChatMessageService_GetMessagesAsync_ShouldReturnEmpty_WhenSessionNotFound
- [x] UT207 - ChatMessageService_GetMessagesAsync_ShouldReturnEmpty_WhenUserNotParticipant
- [x] UT208 - ChatMessageService_GetMessagesAsync_ShouldReturnMappedMessages_WhenUserIsParticipant

### Function: SendMessageAsync (5 tests)
- [x] UT209 - ChatMessageService_SendMessageAsync_ShouldReturnNull_WhenSessionNotFound
- [x] UT210 - ChatMessageService_SendMessageAsync_ShouldReturnNull_WhenSessionClosed
- [x] UT211 - ChatMessageService_SendMessageAsync_ShouldReturnNull_WhenUserNotParticipant
- [x] UT212 - ChatMessageService_SendMessageAsync_ShouldAutoCloseSessionAndReturnNull_WhenBookingCompleted
- [x] UT213 - ChatMessageService_SendMessageAsync_ShouldPersistAndReturnMessage_WhenValid

## 11) NotificationService (8 tests)

### Function: SendNotificationAsync (2 tests)
- [x] UT214 - NotificationService_SendNotificationAsync_ShouldPersistNotification_BeforeRealtimePublish
- [x] UT215 - NotificationService_SendNotificationAsync_ShouldNotThrow_WhenRealtimePublishFails

### Function: GetUserNotificationsAsync (2 tests)
- [x] UT216 - NotificationService_GetUserNotificationsAsync_ShouldApplyDefaultPagination_WhenModelIsEmpty
- [x] UT217 - NotificationService_GetUserNotificationsAsync_ShouldCapPageSizeTo100_WhenRequestedTooLarge

### Function: MarkAsReadAsync (2 tests)
- [x] UT218 - NotificationService_MarkAsReadAsync_ShouldReturnFalse_WhenRepositoryReturnsFalse
- [x] UT219 - NotificationService_MarkAsReadAsync_ShouldSaveChangesAndReturnTrue_WhenRepositoryReturnsTrue

### Function: MarkAllAsReadAsync (1 tests)
- [x] UT220 - NotificationService_MarkAllAsReadAsync_ShouldSaveChangesAndReturnTrue

### Function: DeleteNotificationAsync (1 tests)
- [x] UT221 - NotificationService_DeleteNotificationAsync_ShouldFollowRepositoryResult_AndSaveOnSuccess

## 12) BlockchainService (10 tests)

### Function: ComputeFileHashAsync (1 tests)
- [x] UT222 - BlockchainService_ComputeFileHashAsync_ShouldReturnHexSha256_With0xPrefix

### Function: ComputeFileHashFromUrlAsync (1 tests)
- [x] UT223 - BlockchainService_ComputeFileHashFromUrlAsync_ShouldReturnHexSha256_With0xPrefix

### Function: RegisterDocumentAsync (2 tests)
- [x] UT224 - BlockchainService_RegisterDocumentAsync_ShouldThrow_WhenTransactionReverted
- [x] UT225 - BlockchainService_RegisterDocumentAsync_ShouldReturnTransactionHash_WhenSuccessful

### Function: AssignDocumentOwnerAsync (3 tests)
- [x] UT226 - BlockchainService_AssignDocumentOwnerAsync_ShouldThrow_WhenFileHashEmpty
- [x] UT227 - BlockchainService_AssignDocumentOwnerAsync_ShouldThrow_WhenInvestorWalletEmpty
- [x] UT228 - BlockchainService_AssignDocumentOwnerAsync_ShouldWrapRevertException_AsInvalidOperation

### Function: VerifyDocumentAsync (2 tests)
- [x] UT229 - BlockchainService_VerifyDocumentAsync_ShouldReturnEmptyTuple_WhenHashNotFoundOnChain
- [x] UT230 - BlockchainService_VerifyDocumentAsync_ShouldReturnStartupTimestampAndOwners_WhenFound

### Function: VerifyProjectDocumentsAsync (1 tests)
- [x] UT231 - BlockchainService_VerifyProjectDocumentsAsync_ShouldAggregateVerifiedAndUnverifiedDocuments

## 13) Background Services + Middleware (13 tests)

### Function: GlobalExceptionMiddleware (7 tests)
- [x] UT232 - GlobalExceptionMiddleware_ShouldMapValidationException_To400BadRequest (tests/AISEP.BLL.Tests/BackgroundServices/BackgroundServicesAndMiddlewareGroupedTests.cs)
- [x] UT233 - GlobalExceptionMiddleware_ShouldMapKeyNotFoundException_To404NotFound (tests/AISEP.BLL.Tests/BackgroundServices/BackgroundServicesAndMiddlewareGroupedTests.cs)
- [x] UT234 - GlobalExceptionMiddleware_ShouldMapForbiddenAccessException_To403Forbidden (tests/AISEP.BLL.Tests/BackgroundServices/BackgroundServicesAndMiddlewareGroupedTests.cs)
- [x] UT235 - GlobalExceptionMiddleware_ShouldMapInvalidOperationException_To409Conflict (tests/AISEP.BLL.Tests/BackgroundServices/BackgroundServicesAndMiddlewareGroupedTests.cs)
- [x] UT236 - GlobalExceptionMiddleware_ShouldMapHttpRequestException_To502BadGateway (tests/AISEP.BLL.Tests/BackgroundServices/BackgroundServicesAndMiddlewareGroupedTests.cs)
- [x] UT237 - GlobalExceptionMiddleware_ShouldMapUnknownException_To500InternalServerError (tests/AISEP.BLL.Tests/BackgroundServices/BackgroundServicesAndMiddlewareGroupedTests.cs)
- [x] UT238 - GlobalExceptionMiddleware_ShouldSkipWritingResponse_WhenResponseHasStarted (tests/AISEP.BLL.Tests/BackgroundServices/BackgroundServicesAndMiddlewareGroupedTests.cs)

### Function: BlockchainOwnershipAssignmentBackgroundService (2 tests)
- [x] UT239 - BlockchainOwnershipAssignmentBackgroundService_ShouldAssignOwnerAndNotify_WhenWorkItemDequeued (tests/AISEP.BLL.Tests/BackgroundServices/BackgroundServicesAndMiddlewareGroupedTests.cs)
- [x] UT240 - BlockchainOwnershipAssignmentBackgroundService_ShouldLogErrorAndContinue_WhenAssignmentFails (tests/AISEP.BLL.Tests/BackgroundServices/BackgroundServicesAndMiddlewareGroupedTests.cs)

### Function: BookingResponseExpiryBackgroundService (1 tests)
- [x] UT241 - BookingResponseExpiryBackgroundService_ShouldInvokeExpirePendingAdvisorResponses_PerCycle (tests/AISEP.BLL.Tests/BackgroundServices/BackgroundServicesAndMiddlewareGroupedTests.cs)

### Function: ConsultingReportDeadlineBackgroundService (1 tests)
- [x] UT242 - ConsultingReportDeadlineBackgroundService_ShouldInvokeProcessReportDeadlines_PerCycle (tests/AISEP.BLL.Tests/BackgroundServices/BackgroundServicesAndMiddlewareGroupedTests.cs)

### Function: SubscriptionExpiryBackgroundService (1 tests)
- [x] UT243 - SubscriptionExpiryBackgroundService_ShouldMarkExpiredAndRevokePremium_WhenNoActiveSubscriptionLeft (tests/AISEP.BLL.Tests/BackgroundServices/BackgroundServicesAndMiddlewareGroupedTests.cs)

### Function: ProjectAdvisorAutoAssignBackgroundService (1 tests)
- [x] UT244 - ProjectAdvisorAutoAssignBackgroundService_ShouldInvokeAutoAssignUnassignedApprovedProjects_PerCycle (tests/AISEP.BLL.Tests/BackgroundServices/BackgroundServicesAndMiddlewareGroupedTests.cs)

## 14) Critical API Controllers (20 tests)

### Function: Register (4 tests)
- [ ] UT245 - AuthController_Register_ShouldReturnBadRequest_WhenModelStateInvalid
- [ ] UT246 - AuthController_Register_ShouldReturnBadRequest_WhenPasswordAndConfirmPasswordMismatch
- [ ] UT247 - AuthController_Register_ShouldReturnBadRequest_WhenServiceReturnsFailure
- [ ] UT248 - AuthController_Register_ShouldReturnOkWithUserInfo_WhenServiceSucceeds

### Function: Login (2 tests)
- [ ] UT249 - AuthController_Login_ShouldReturnUnauthorized_WhenServiceReturnsFailure
- [ ] UT250 - AuthController_Login_ShouldReturnOk_WhenServiceSucceeds

### Function: CheckoutBooking (2 tests)
- [ ] UT251 - PaymentController_CheckoutBooking_ShouldReturn404_WhenServiceThrowsKeyNotFound
- [ ] UT252 - PaymentController_CheckoutBooking_ShouldReturn400_WhenServiceThrowsInvalidOperation

### Function: CheckoutSubscription (1 tests)
- [ ] UT253 - PaymentController_CheckoutSubscription_ShouldReturn404_WhenServiceThrowsKeyNotFound

### Function: SePayWebhook (1 tests)
- [ ] UT254 - PaymentController_SePayWebhook_ShouldReturn400_WhenServiceThrowsInvalidOperation

### Function: GetDeals (3 tests)
- [ ] UT255 - DealsController_GetDeals_ShouldCallGetInvestorDeals_WhenCurrentRoleIsInvestor
- [ ] UT256 - DealsController_GetDeals_ShouldCallGetStartupDeals_WhenCurrentRoleIsStartup
- [ ] UT257 - DealsController_GetDeals_ShouldCallGetDeals_WhenCurrentRoleIsStaffOrAdmin

### Function: RespondDeal (1 tests)
- [ ] UT258 - DealsController_RespondDeal_ShouldThrow_WhenIsAcceptedMissing

### Function: CreateBooking (2 tests)
- [ ] UT259 - BookingController_CreateBooking_ShouldReturnCreated_WhenServiceReturnsBooking
- [ ] UT260 - BookingController_CreateBooking_ShouldReturnBadRequest_WhenServiceReturnsNull

### Function: GetById (1 tests)
- [ ] UT261 - ConsultingReportController_GetById_ShouldReturn404_WhenReportMissing

### Function: VerifyBlockchain (1 tests)
- [ ] UT262 - ProjectsController_VerifyBlockchain_ShouldReturn400_WhenNotFullyVerified

### Function: Upload (1 tests)
- [ ] UT263 - DocumentController_Upload_ShouldReturn409_WhenServiceThrowsInvalidOperation

### Function: VerifyDocument (1 tests)
- [ ] UT264 - DocumentController_VerifyDocument_ShouldReturn502_WhenServiceThrowsHttpRequestException

## 15) Core Validators (14 tests)

### Function: RegisterRequestValidator (2 tests)
- [ ] UT265 - RegisterRequestValidator_ShouldPass_WhenRequestValid
- [ ] UT266 - RegisterRequestValidator_ShouldFail_WhenPasswordNotComplex

### Function: InvestorSignContractDtoValidator (2 tests)
- [ ] UT267 - InvestorSignContractDtoValidator_ShouldPass_WhenRequestValid
- [ ] UT268 - InvestorSignContractDtoValidator_ShouldFail_WhenSignatureMissing

### Function: CreateBookingRequestValidator (2 tests)
- [ ] UT269 - CreateBookingRequestValidator_ShouldPass_WhenSlotIdsUniqueAndValid
- [ ] UT270 - CreateBookingRequestValidator_ShouldFail_WhenSlotIdsContainDuplicates

### Function: CreateProjectRequestValidator (2 tests)
- [ ] UT271 - CreateProjectRequestValidator_ShouldPass_WhenGrowthFieldsComplete
- [ ] UT272 - CreateProjectRequestValidator_ShouldFail_WhenGrowthRevenueMissingOrNonPositive

### Function: UploadDocumentRequestValidator (2 tests)
- [ ] UT273 - UploadDocumentRequestValidator_ShouldPass_WhenPdfWithinSizeLimit
- [ ] UT274 - UploadDocumentRequestValidator_ShouldFail_WhenMimeTypeNotAllowed

### Function: CreateConsultingReportRequestValidator (2 tests)
- [ ] UT275 - CreateConsultingReportRequestValidator_ShouldPass_WhenRequiredFieldsProvided
- [ ] UT276 - CreateConsultingReportRequestValidator_ShouldFail_WhenRequiredFieldsMissing

### Function: SendMessageRequestValidator (2 tests)
- [ ] UT277 - SendMessageRequestValidator_ShouldPass_WhenContentValid
- [ ] UT278 - SendMessageRequestValidator_ShouldFail_WhenContentEmpty

## Completion Notes
- Add actual test class path beside each item during implementation.
- If one test is intentionally skipped, document reason in PR.

