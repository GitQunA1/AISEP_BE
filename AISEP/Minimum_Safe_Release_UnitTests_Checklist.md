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
- [ ] UT001 - AuthService_RegisterAsync_ShouldFail_WhenEmailAlreadyRegistered
- [ ] UT002 - AuthService_RegisterAsync_ShouldFail_WhenRoleIsAdminOrStaff
- [ ] UT003 - AuthService_RegisterAsync_ShouldFail_WhenCreateUserReturnsErrors
- [ ] UT004 - AuthService_RegisterAsync_ShouldSendEmailConfirmation_WhenRegistrationSucceeds
- [ ] UT005 - AuthService_RegisterAsync_ShouldReturnUserIdAndEmail_WhenRegistrationSucceeds

### Function: ConfirmEmailAsync (4 tests)
- [ ] UT006 - AuthService_ConfirmEmailAsync_ShouldFail_WhenUserNotFound
- [ ] UT007 - AuthService_ConfirmEmailAsync_ShouldReturnAlreadyConfirmed_WhenEmailAlreadyConfirmed
- [ ] UT008 - AuthService_ConfirmEmailAsync_ShouldFail_WhenTokenInvalid
- [ ] UT009 - AuthService_ConfirmEmailAsync_ShouldSetUserActive_WhenConfirmationSucceeds

### Function: ResendConfirmationAsync (4 tests)
- [ ] UT010 - AuthService_ResendConfirmationAsync_ShouldReturnGenericSuccess_WhenUserNotFound
- [ ] UT011 - AuthService_ResendConfirmationAsync_ShouldFail_WhenEmailAlreadyConfirmed
- [ ] UT012 - AuthService_ResendConfirmationAsync_ShouldFail_WhenEmailSendingThrows
- [ ] UT013 - AuthService_ResendConfirmationAsync_ShouldSucceed_WhenEmailSent

### Function: LoginAsync (7 tests)
- [ ] UT014 - AuthService_LoginAsync_ShouldFail_WhenUserNotFound
- [ ] UT015 - AuthService_LoginAsync_ShouldFail_WhenUserIsBanned
- [ ] UT016 - AuthService_LoginAsync_ShouldFail_WhenEmailNotConfirmed
- [ ] UT017 - AuthService_LoginAsync_ShouldFail_WhenAccountIsLockedOut
- [ ] UT018 - AuthService_LoginAsync_ShouldFail_WhenPasswordInvalid
- [ ] UT019 - AuthService_LoginAsync_ShouldReturnTokens_WhenCredentialsValid
- [ ] UT020 - AuthService_LoginAsync_ShouldPersistRefreshToken_WhenCredentialsValid

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
- [ ] UT028 - AuthService_LogoutAsync_ShouldRevokeAllActiveTokensAndSignOut

### Function: ForgotPasswordAsync (3 tests)
- [ ] UT029 - AuthService_ForgotPasswordAsync_ShouldReturnGenericSuccess_WhenEmailEmpty
- [ ] UT030 - AuthService_ForgotPasswordAsync_ShouldFail_WhenEmailSendingThrows
- [ ] UT031 - AuthService_ForgotPasswordAsync_ShouldSendResetEmail_WhenUserExists

### Function: ResetPasswordAsync (3 tests)
- [ ] UT032 - AuthService_ResetPasswordAsync_ShouldFail_WhenUserNotFound
- [ ] UT033 - AuthService_ResetPasswordAsync_ShouldFallbackToRawToken_WhenBase64DecodeFails
- [ ] UT034 - AuthService_ResetPasswordAsync_ShouldRevokeAllActiveTokens_WhenResetSucceeds

### Function: ChangePasswordAsync (3 tests)
- [ ] UT035 - AuthService_ChangePasswordAsync_ShouldFail_WhenUserNotFound
- [ ] UT036 - AuthService_ChangePasswordAsync_ShouldReturnErrors_WhenIdentityChangeFails
- [ ] UT037 - AuthService_ChangePasswordAsync_ShouldRevokeAllActiveTokens_WhenChangeSucceeds

## 2) PaymentService (36 tests)

### Function: GetInvestorPackagesAsync (1 tests)
- [ ] UT038 - PaymentService_GetInvestorPackagesAsync_ShouldReturnOnlyInvestorPackages

### Function: GetStartupPackagesAsync (1 tests)
- [ ] UT039 - PaymentService_GetStartupPackagesAsync_ShouldReturnOnlyStartupPackages

### Function: CheckoutSubscriptionAsync (8 tests)
- [ ] UT040 - PaymentService_CheckoutSubscriptionAsync_ShouldThrow_WhenPackageIdIsNotPositive
- [ ] UT041 - PaymentService_CheckoutSubscriptionAsync_ShouldThrow_WhenPackageNotFound
- [ ] UT042 - PaymentService_CheckoutSubscriptionAsync_ShouldThrow_WhenUserNotFound
- [ ] UT043 - PaymentService_CheckoutSubscriptionAsync_ShouldThrow_WhenPackageRoleMismatch
- [ ] UT044 - PaymentService_CheckoutSubscriptionAsync_ShouldReusePendingTransaction_WhenPendingNotExpired
- [ ] UT045 - PaymentService_CheckoutSubscriptionAsync_ShouldFailOldPendingAndCreateNew_WhenPendingExpired
- [ ] UT046 - PaymentService_CheckoutSubscriptionAsync_ShouldGeneratePaymentCodeWithPrefix_WhenCreated
- [ ] UT047 - PaymentService_CheckoutSubscriptionAsync_ShouldReturnQrCodeUrl_WhenCreated

### Function: CheckoutBookingAsync (4 tests)
- [ ] UT048 - PaymentService_CheckoutBookingAsync_ShouldThrow_WhenBookingIdIsNotPositive
- [ ] UT049 - PaymentService_CheckoutBookingAsync_ShouldThrow_WhenPayableBookingNotFound
- [ ] UT050 - PaymentService_CheckoutBookingAsync_ShouldReusePendingTransaction_WhenPendingNotExpired
- [ ] UT051 - PaymentService_CheckoutBookingAsync_ShouldCreatePendingTransaction_WhenNoPendingExists

### Function: UpdatePackageAsync (7 tests)
- [ ] UT052 - PaymentService_UpdatePackageAsync_ShouldThrow_WhenPackageIdIsNotPositive
- [ ] UT053 - PaymentService_UpdatePackageAsync_ShouldThrow_WhenPackageNotFound
- [ ] UT054 - PaymentService_UpdatePackageAsync_ShouldThrow_WhenPackageTargetRoleNotSupported
- [ ] UT055 - PaymentService_UpdatePackageAsync_ShouldThrow_WhenPriceIsNotPositive
- [ ] UT056 - PaymentService_UpdatePackageAsync_ShouldThrow_WhenDurationMonthsIsNotPositive
- [ ] UT057 - PaymentService_UpdatePackageAsync_ShouldThrow_WhenPackageNameIsEmpty
- [ ] UT058 - PaymentService_UpdatePackageAsync_ShouldPersistFields_WhenInputValid

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
- [ ] UT074 - DealService_CreateDealAsync_ShouldThrow_WhenProjectIdIsNotPositive
- [ ] UT075 - DealService_CreateDealAsync_ShouldThrow_WhenInvestorNotFound
- [ ] UT076 - DealService_CreateDealAsync_ShouldThrow_WhenProjectNotFound
- [ ] UT077 - DealService_CreateDealAsync_ShouldThrow_WhenBlockingDealExists
- [ ] UT078 - DealService_CreateDealAsync_ShouldSetPendingFlags_WhenCreated
- [ ] UT079 - DealService_CreateDealAsync_ShouldSendNotificationToStartup_WhenCreated

### Function: GetDealsAsync (1 tests)
- [ ] UT080 - DealService_GetDealsAsync_ShouldReturnPagedDeals

### Function: GetInvestorDealsAsync (1 tests)
- [ ] UT081 - DealService_GetInvestorDealsAsync_ShouldFilterByInvestorId

### Function: GetStartupDealsAsync (1 tests)
- [ ] UT082 - DealService_GetStartupDealsAsync_ShouldFilterByStartupId

### Function: RespondDealAsync (6 tests)
- [ ] UT083 - DealService_RespondDealAsync_ShouldThrow_WhenDealNotFound
- [ ] UT084 - DealService_RespondDealAsync_ShouldThrowForbidden_WhenStartupDoesNotOwnProject
- [ ] UT085 - DealService_RespondDealAsync_ShouldThrow_WhenDealStatusIsNotPending
- [ ] UT086 - DealService_RespondDealAsync_ShouldSetConfirmed_WhenAccepted
- [ ] UT087 - DealService_RespondDealAsync_ShouldSetRejected_WhenRejected
- [ ] UT088 - DealService_RespondDealAsync_ShouldNotifyInvestor_WhenResponded

### Function: GetContractPreviewForInvestorAsync (3 tests)
- [ ] UT089 - DealService_GetContractPreviewForInvestorAsync_ShouldThrowForbidden_WhenInvestorDoesNotOwnDeal
- [ ] UT090 - DealService_GetContractPreviewForInvestorAsync_ShouldThrow_WhenStatusNotInSigningFlow
- [ ] UT091 - DealService_GetContractPreviewForInvestorAsync_ShouldReturnHtml_WhenValid

### Function: InvestorSignContractAsync (4 tests)
- [ ] UT092 - DealService_InvestorSignContractAsync_ShouldThrow_WhenFinalAmountIsNotPositive
- [ ] UT093 - DealService_InvestorSignContractAsync_ShouldThrow_WhenFinalEquityPercentageIsNegative
- [ ] UT094 - DealService_InvestorSignContractAsync_ShouldThrow_WhenDealStatusIsNotConfirmed
- [ ] UT095 - DealService_InvestorSignContractAsync_ShouldSetWaitingForStartupSignature_WhenSuccessful

### Function: StartupSignContractAsync (6 tests)
- [ ] UT096 - DealService_StartupSignContractAsync_ShouldThrow_WhenDealStatusIsNotWaitingForStartupSignature
- [ ] UT097 - DealService_StartupSignContractAsync_ShouldThrow_WhenInvestorSignatureMissing
- [ ] UT098 - DealService_StartupSignContractAsync_ShouldThrow_WhenPdfGenerationFails
- [ ] UT099 - DealService_StartupSignContractAsync_ShouldThrow_WhenPdfUploadFails
- [ ] UT100 - DealService_StartupSignContractAsync_ShouldFallbackDirectBlockchainCall_WhenQueueFails
- [ ] UT101 - DealService_StartupSignContractAsync_ShouldSetContractSignedAndNotifyInvestor_WhenSuccessful

### Function: StartupRejectContractAsync (2 tests)
- [ ] UT102 - DealService_StartupRejectContractAsync_ShouldThrow_WhenDealStatusIsNotWaitingForStartupSignature
- [ ] UT103 - DealService_StartupRejectContractAsync_ShouldSetRejectedAndClearSignatures_WhenSuccessful

### Function: GetContractStatusForInvestorAsync (1 tests)
- [ ] UT104 - DealService_GetContractStatusForInvestorAsync_ShouldThrowForbidden_WhenInvestorDoesNotOwnDeal

### Function: GetOwnershipAssignmentStatusAsync (3 tests)
- [ ] UT105 - DealService_GetOwnershipAssignmentStatusAsync_ShouldThrow_WhenNoRegisteredProjectDocumentExists
- [ ] UT106 - DealService_GetOwnershipAssignmentStatusAsync_ShouldReturnAssigned_WhenInvestorWalletExistsOnChain
- [ ] UT107 - DealService_GetOwnershipAssignmentStatusAsync_ShouldReturnUnassigned_WhenInvestorWalletMissingOnChain

## 4) BookingService (22 tests)

### Function: CreateBookingAsync (15 tests)
- [ ] UT108 - BookingService_CreateBookingAsync_ShouldThrow_WhenNoSlotSelected
- [ ] UT109 - BookingService_CreateBookingAsync_ShouldThrow_WhenAdvisorNotFound
- [ ] UT110 - BookingService_CreateBookingAsync_ShouldThrow_WhenProjectNotFound
- [ ] UT111 - BookingService_CreateBookingAsync_ShouldThrow_WhenProjectHasNoAdvisorAssignment
- [ ] UT112 - BookingService_CreateBookingAsync_ShouldThrow_WhenSelectedSlotIdsContainMissingItems
- [ ] UT113 - BookingService_CreateBookingAsync_ShouldThrow_WhenSelectedSlotsBelongToDifferentAdvisor
- [ ] UT114 - BookingService_CreateBookingAsync_ShouldThrow_WhenAnySelectedSlotNotAvailable
- [ ] UT115 - BookingService_CreateBookingAsync_ShouldThrow_WhenAnySelectedSlotInPast
- [ ] UT116 - BookingService_CreateBookingAsync_ShouldThrow_WhenSelectedSlotsAreNotConsecutive
- [ ] UT117 - BookingService_CreateBookingAsync_ShouldThrow_WhenBookingLessThan12HoursInAdvance
- [ ] UT118 - BookingService_CreateBookingAsync_ShouldThrow_WhenFreeRebookFromComplaintAlreadyUsed
- [ ] UT119 - BookingService_CreateBookingAsync_ShouldThrow_WhenPremiumFreeBookingDurationExceeds3Hours
- [ ] UT120 - BookingService_CreateBookingAsync_ShouldThrow_WhenPremiumFreeQuotaNotAvailable
- [ ] UT121 - BookingService_CreateBookingAsync_ShouldCreateBookingAndReserveSlots_WhenValid
- [ ] UT122 - BookingService_CreateBookingAsync_ShouldNotifyAdvisor_WhenBookingCreated

### Function: ApproveBookingAsync (4 tests)
- [ ] UT123 - BookingService_ApproveBookingAsync_ShouldThrow_WhenBookingNotFound
- [ ] UT124 - BookingService_ApproveBookingAsync_ShouldThrow_WhenAdvisorResponseWindowExpired
- [ ] UT125 - BookingService_ApproveBookingAsync_ShouldSetConfirmed_WhenPaymentWaivedOrPriceZero
- [ ] UT126 - BookingService_ApproveBookingAsync_ShouldSetApprovedAwaitingPayment_WhenPaymentRequired

### Function: RejectBookingAsync (2 tests)
- [ ] UT127 - BookingService_RejectBookingAsync_ShouldThrow_WhenStatusCannotBeRejected
- [ ] UT128 - BookingService_RejectBookingAsync_ShouldReleaseSlotsAndNotifyCustomer_WhenRejected

### Function: ExpirePendingAdvisorResponsesAsync (1 tests)
- [ ] UT129 - BookingService_ExpirePendingAdvisorResponsesAsync_ShouldMarkNoResponseAndNotifyCustomer

## 5) ConsultingReportService (19 tests)

### Function: CreateAsync (8 tests)
- [ ] UT130 - ConsultingReportService_CreateAsync_ShouldThrowForbidden_WhenCurrentUserIsNotAdvisor
- [ ] UT131 - ConsultingReportService_CreateAsync_ShouldThrow_WhenBookingNotFound
- [ ] UT132 - ConsultingReportService_CreateAsync_ShouldThrowForbidden_WhenAdvisorNotAssignedToBooking
- [ ] UT133 - ConsultingReportService_CreateAsync_ShouldThrow_WhenBookingStatusIsNotConfirmed
- [ ] UT134 - ConsultingReportService_CreateAsync_ShouldThrow_WhenSubmissionWindowExpired
- [ ] UT135 - ConsultingReportService_CreateAsync_ShouldCreateSubmittedReport_WhenNoExistingReport
- [ ] UT136 - ConsultingReportService_CreateAsync_ShouldThrow_WhenExistingReportIsNotRevisionRequested
- [ ] UT137 - ConsultingReportService_CreateAsync_ShouldUpdateReport_WhenRevisionRequestedAndWithinDeadline

### Function: ApproveAsync (3 tests)
- [ ] UT138 - ConsultingReportService_ApproveAsync_ShouldThrow_WhenReportStatusIsNotSubmitted
- [ ] UT139 - ConsultingReportService_ApproveAsync_ShouldCompleteBookingAndCloseChat_WhenApproved
- [ ] UT140 - ConsultingReportService_ApproveAsync_ShouldDisburseAdvisorPayout_WhenApproved

### Function: RequestRevisionAsync (3 tests)
- [ ] UT141 - ConsultingReportService_RequestRevisionAsync_ShouldThrow_WhenReportStatusIsNotSubmitted
- [ ] UT142 - ConsultingReportService_RequestRevisionAsync_ShouldEscalateToStaff_WhenRevisionCountReachedMax
- [ ] UT143 - ConsultingReportService_RequestRevisionAsync_ShouldSetRevisionRequestedAndAdvisorDue_WhenUnderLimit

### Function: AcceptComplaintByStaffAsync (2 tests)
- [ ] UT144 - ConsultingReportService_AcceptComplaintByStaffAsync_ShouldThrow_WhenReportStatusIsNotEscalated
- [ ] UT145 - ConsultingReportService_AcceptComplaintByStaffAsync_ShouldSkipPayoutAndRefundQuota_WhenAccepted

### Function: RejectComplaintByStaffAsync (1 tests)
- [ ] UT146 - ConsultingReportService_RejectComplaintByStaffAsync_ShouldDisbursePayout_WhenRejected

### Function: ProcessReportDeadlinesAsync (2 tests)
- [ ] UT147 - ConsultingReportService_ProcessReportDeadlinesAsync_ShouldAutoApprove_WhenStartupReviewTimesOut
- [ ] UT148 - ConsultingReportService_ProcessReportDeadlinesAsync_ShouldEscalateToStaff_WhenAdvisorRevisionTimesOut

## 6) ProjectService (15 tests)

### Function: GetProjectByIdAsync (6 tests)
- [ ] UT149 - ProjectService_GetProjectByIdAsync_ShouldThrow_WhenProjectNotFound
- [ ] UT150 - ProjectService_GetProjectByIdAsync_ShouldBypassQuota_WhenRoleIsStaffOrAdmin
- [ ] UT151 - ProjectService_GetProjectByIdAsync_ShouldBypassQuota_WhenStartupOwnsProject
- [ ] UT152 - ProjectService_GetProjectByIdAsync_ShouldThrow_WhenNoActiveSubscriptionForQuotaRoles
- [ ] UT153 - ProjectService_GetProjectByIdAsync_ShouldThrow_WhenProjectViewQuotaExceeded
- [ ] UT154 - ProjectService_GetProjectByIdAsync_ShouldConsumeQuotaAndUnlockProject_WhenFirstView

### Function: CreateProjectAsync (2 tests)
- [ ] UT155 - ProjectService_CreateProjectAsync_ShouldThrow_WhenStartupProfileNotFound
- [ ] UT156 - ProjectService_CreateProjectAsync_ShouldSetDraftAndUploadImage_WhenValidRequest

### Function: UpdateProjectAsync (4 tests)
- [ ] UT157 - ProjectService_UpdateProjectAsync_ShouldThrow_WhenProjectNotFound
- [ ] UT158 - ProjectService_UpdateProjectAsync_ShouldThrowForbidden_WhenStartupDoesNotOwnProject
- [ ] UT159 - ProjectService_UpdateProjectAsync_ShouldThrow_WhenStatusIsNotDraftOrRejected
- [ ] UT160 - ProjectService_UpdateProjectAsync_ShouldMoveRejectedToDraft_BeforeApplyingUpdates

### Function: SubmitProjectAsync (1 tests)
- [ ] UT161 - ProjectService_SubmitProjectAsync_ShouldThrow_WhenProjectStatusIsNotDraft

### Function: RejectProjectAsync (2 tests)
- [ ] UT162 - ProjectService_RejectProjectAsync_ShouldThrow_WhenProjectStatusIsNotPending
- [ ] UT163 - ProjectService_RejectProjectAsync_ShouldSetRejectedMetadata_WhenSuccessful

## 7) DocumentService (18 tests)

### Function: UploadDocumentAsync (7 tests)
- [ ] UT164 - DocumentService_UploadDocumentAsync_ShouldThrow_WhenProjectNotFound
- [ ] UT165 - DocumentService_UploadDocumentAsync_ShouldThrowUnauthorized_WhenStartupDoesNotOwnProject
- [ ] UT166 - DocumentService_UploadDocumentAsync_ShouldThrow_WhenProjectStatusIsNotDraft
- [ ] UT167 - DocumentService_UploadDocumentAsync_ShouldThrow_WhenDuplicateFileHashExistsInDatabase
- [ ] UT168 - DocumentService_UploadDocumentAsync_ShouldThrow_WhenFileHashAlreadyExistsOnBlockchain
- [ ] UT169 - DocumentService_UploadDocumentAsync_ShouldWrapError_WhenBlockchainVerifyFailsUnexpectedly
- [ ] UT170 - DocumentService_UploadDocumentAsync_ShouldPersistDocumentWithHashAndUrl_WhenValid

### Function: GetByIdAsync (2 tests)
- [ ] UT171 - DocumentService_GetByIdAsync_ShouldReturnNull_WhenDocumentNotFound
- [ ] UT172 - DocumentService_GetByIdAsync_ShouldThrowUnauthorized_WhenUserCannotViewProjectDocuments

### Function: DeleteAsync (4 tests)
- [ ] UT173 - DocumentService_DeleteAsync_ShouldReturnFalse_WhenDocumentNotFound
- [ ] UT174 - DocumentService_DeleteAsync_ShouldThrowUnauthorized_WhenStartupCannotDeleteForeignDocument
- [ ] UT175 - DocumentService_DeleteAsync_ShouldThrow_WhenProjectIsLockedByApprovedStatus
- [ ] UT176 - DocumentService_DeleteAsync_ShouldDeleteDocument_WhenAllowed

### Function: VerifyDocumentAsync (2 tests)
- [ ] UT177 - DocumentService_VerifyDocumentAsync_ShouldThrow_WhenDocumentNotFound
- [ ] UT178 - DocumentService_VerifyDocumentAsync_ShouldThrow_WhenDocumentNotRegisteredOnBlockchain

### Function: ApproveProjectAsync (3 tests)
- [ ] UT179 - DocumentService_ApproveProjectAsync_ShouldThrow_WhenProjectStatusIsNotPending
- [ ] UT180 - DocumentService_ApproveProjectAsync_ShouldThrow_WhenNoProjectDocumentExists
- [ ] UT181 - DocumentService_ApproveProjectAsync_ShouldRegisterHashAndApproveProject_WhenValid

## 8) ConnectionService (14 tests)

### Function: GetInvestorRequestsAsync (2 tests)
- [ ] UT182 - ConnectionService_GetInvestorRequestsAsync_ShouldThrow_WhenStatusFilterInvalid
- [ ] UT183 - ConnectionService_GetInvestorRequestsAsync_ShouldApplyStatusFilter_WhenProvided

### Function: GetStartupRequestsAsync (1 tests)
- [ ] UT184 - ConnectionService_GetStartupRequestsAsync_ShouldApplyStatusFilter_WhenProvided

### Function: CreateRequestAsync (4 tests)
- [ ] UT185 - ConnectionService_CreateRequestAsync_ShouldThrow_WhenInvestorNotFound
- [ ] UT186 - ConnectionService_CreateRequestAsync_ShouldThrow_WhenProjectNotFound
- [ ] UT187 - ConnectionService_CreateRequestAsync_ShouldThrow_WhenPendingRequestAlreadyExists
- [ ] UT188 - ConnectionService_CreateRequestAsync_ShouldPersistPendingRequestAndNotifyStartup_WhenValid

### Function: RespondToRequestAsync (6 tests)
- [ ] UT189 - ConnectionService_RespondToRequestAsync_ShouldThrow_WhenRequestNotFound
- [ ] UT190 - ConnectionService_RespondToRequestAsync_ShouldThrowForbidden_WhenStartupDoesNotOwnProject
- [ ] UT191 - ConnectionService_RespondToRequestAsync_ShouldThrow_WhenRequestStatusIsNotPending
- [ ] UT192 - ConnectionService_RespondToRequestAsync_ShouldOpenChatSession_WhenAccepted
- [ ] UT193 - ConnectionService_RespondToRequestAsync_ShouldThrow_WhenAcceptedButChatSessionNotCreated
- [ ] UT194 - ConnectionService_RespondToRequestAsync_ShouldNotifyInvestorWithConnectionReference_WhenRejected

### Function: GetFounderContactAsync (1 tests)
- [ ] UT195 - ConnectionService_GetFounderContactAsync_ShouldThrowUnauthorized_WhenNoAcceptedConnectionExists

## 9) ChatSessionService (10 tests)

### Function: OpenSessionAsync (5 tests)
- [ ] UT196 - ChatSessionService_OpenSessionAsync_ShouldReturnNull_WhenBookingNotFound
- [ ] UT197 - ChatSessionService_OpenSessionAsync_ShouldReturnNull_WhenUserNotBookingParticipant
- [ ] UT198 - ChatSessionService_OpenSessionAsync_ShouldReturnNull_WhenBookingStatusIsNotConfirmed
- [ ] UT199 - ChatSessionService_OpenSessionAsync_ShouldReturnExistingSession_WhenAlreadyExists
- [ ] UT200 - ChatSessionService_OpenSessionAsync_ShouldCreateSession_WhenNotExistsAndConfirmed

### Function: OpenSessionByConnectionRequestAsync (2 tests)
- [ ] UT201 - ChatSessionService_OpenSessionByConnectionRequestAsync_ShouldReturnNull_WhenRequestInvalidOrNotAccepted
- [ ] UT202 - ChatSessionService_OpenSessionByConnectionRequestAsync_ShouldReturnExistingSession_WhenAlreadyExists

### Function: GetSessionAsync (1 tests)
- [ ] UT203 - ChatSessionService_GetSessionAsync_ShouldReturnNull_WhenUserNotParticipant

### Function: CloseSessionAsync (2 tests)
- [ ] UT204 - ChatSessionService_CloseSessionAsync_ShouldReturnFalse_WhenSessionMissingClosedOrUnauthorized
- [ ] UT205 - ChatSessionService_CloseSessionAsync_ShouldCloseSessionAndSetEndTime_WhenAuthorized

## 10) ChatMessageService (8 tests)

### Function: GetMessagesAsync (3 tests)
- [ ] UT206 - ChatMessageService_GetMessagesAsync_ShouldReturnEmpty_WhenSessionNotFound
- [ ] UT207 - ChatMessageService_GetMessagesAsync_ShouldReturnEmpty_WhenUserNotParticipant
- [ ] UT208 - ChatMessageService_GetMessagesAsync_ShouldReturnMappedMessages_WhenUserIsParticipant

### Function: SendMessageAsync (5 tests)
- [ ] UT209 - ChatMessageService_SendMessageAsync_ShouldReturnNull_WhenSessionNotFound
- [ ] UT210 - ChatMessageService_SendMessageAsync_ShouldReturnNull_WhenSessionClosed
- [ ] UT211 - ChatMessageService_SendMessageAsync_ShouldReturnNull_WhenUserNotParticipant
- [ ] UT212 - ChatMessageService_SendMessageAsync_ShouldAutoCloseSessionAndReturnNull_WhenBookingCompleted
- [ ] UT213 - ChatMessageService_SendMessageAsync_ShouldPersistAndReturnMessage_WhenValid

## 11) NotificationService (8 tests)

### Function: SendNotificationAsync (2 tests)
- [ ] UT214 - NotificationService_SendNotificationAsync_ShouldPersistNotification_BeforeRealtimePublish
- [ ] UT215 - NotificationService_SendNotificationAsync_ShouldNotThrow_WhenRealtimePublishFails

### Function: GetUserNotificationsAsync (2 tests)
- [ ] UT216 - NotificationService_GetUserNotificationsAsync_ShouldApplyDefaultPagination_WhenModelIsEmpty
- [ ] UT217 - NotificationService_GetUserNotificationsAsync_ShouldCapPageSizeTo100_WhenRequestedTooLarge

### Function: MarkAsReadAsync (2 tests)
- [ ] UT218 - NotificationService_MarkAsReadAsync_ShouldReturnFalse_WhenRepositoryReturnsFalse
- [ ] UT219 - NotificationService_MarkAsReadAsync_ShouldSaveChangesAndReturnTrue_WhenRepositoryReturnsTrue

### Function: MarkAllAsReadAsync (1 tests)
- [ ] UT220 - NotificationService_MarkAllAsReadAsync_ShouldSaveChangesAndReturnTrue

### Function: DeleteNotificationAsync (1 tests)
- [ ] UT221 - NotificationService_DeleteNotificationAsync_ShouldFollowRepositoryResult_AndSaveOnSuccess

## 12) BlockchainService (10 tests)

### Function: ComputeFileHashAsync (1 tests)
- [ ] UT222 - BlockchainService_ComputeFileHashAsync_ShouldReturnHexSha256_With0xPrefix

### Function: ComputeFileHashFromUrlAsync (1 tests)
- [ ] UT223 - BlockchainService_ComputeFileHashFromUrlAsync_ShouldReturnHexSha256_With0xPrefix

### Function: RegisterDocumentAsync (2 tests)
- [ ] UT224 - BlockchainService_RegisterDocumentAsync_ShouldThrow_WhenTransactionReverted
- [ ] UT225 - BlockchainService_RegisterDocumentAsync_ShouldReturnTransactionHash_WhenSuccessful

### Function: AssignDocumentOwnerAsync (3 tests)
- [ ] UT226 - BlockchainService_AssignDocumentOwnerAsync_ShouldThrow_WhenFileHashEmpty
- [ ] UT227 - BlockchainService_AssignDocumentOwnerAsync_ShouldThrow_WhenInvestorWalletEmpty
- [ ] UT228 - BlockchainService_AssignDocumentOwnerAsync_ShouldWrapRevertException_AsInvalidOperation

### Function: VerifyDocumentAsync (2 tests)
- [ ] UT229 - BlockchainService_VerifyDocumentAsync_ShouldReturnEmptyTuple_WhenHashNotFoundOnChain
- [ ] UT230 - BlockchainService_VerifyDocumentAsync_ShouldReturnStartupTimestampAndOwners_WhenFound

### Function: VerifyProjectDocumentsAsync (1 tests)
- [ ] UT231 - BlockchainService_VerifyProjectDocumentsAsync_ShouldAggregateVerifiedAndUnverifiedDocuments

## 13) Background Services + Middleware (13 tests)

### Function: GlobalExceptionMiddleware (7 tests)
- [ ] UT232 - GlobalExceptionMiddleware_ShouldMapValidationException_To400BadRequest
- [ ] UT233 - GlobalExceptionMiddleware_ShouldMapKeyNotFoundException_To404NotFound
- [ ] UT234 - GlobalExceptionMiddleware_ShouldMapForbiddenAccessException_To403Forbidden
- [ ] UT235 - GlobalExceptionMiddleware_ShouldMapInvalidOperationException_To409Conflict
- [ ] UT236 - GlobalExceptionMiddleware_ShouldMapHttpRequestException_To502BadGateway
- [ ] UT237 - GlobalExceptionMiddleware_ShouldMapUnknownException_To500InternalServerError
- [ ] UT238 - GlobalExceptionMiddleware_ShouldSkipWritingResponse_WhenResponseHasStarted

### Function: BlockchainOwnershipAssignmentBackgroundService (2 tests)
- [ ] UT239 - BlockchainOwnershipAssignmentBackgroundService_ShouldAssignOwnerAndNotify_WhenWorkItemDequeued
- [ ] UT240 - BlockchainOwnershipAssignmentBackgroundService_ShouldLogErrorAndContinue_WhenAssignmentFails

### Function: BookingResponseExpiryBackgroundService (1 tests)
- [ ] UT241 - BookingResponseExpiryBackgroundService_ShouldInvokeExpirePendingAdvisorResponses_PerCycle

### Function: ConsultingReportDeadlineBackgroundService (1 tests)
- [ ] UT242 - ConsultingReportDeadlineBackgroundService_ShouldInvokeProcessReportDeadlines_PerCycle

### Function: SubscriptionExpiryBackgroundService (1 tests)
- [ ] UT243 - SubscriptionExpiryBackgroundService_ShouldMarkExpiredAndRevokePremium_WhenNoActiveSubscriptionLeft

### Function: ProjectAdvisorAutoAssignBackgroundService (1 tests)
- [ ] UT244 - ProjectAdvisorAutoAssignBackgroundService_ShouldInvokeAutoAssignUnassignedApprovedProjects_PerCycle

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

