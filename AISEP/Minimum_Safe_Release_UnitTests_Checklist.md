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
- [ ] UT001 - AuthService_RegisterAsync_ShouldFail_WhenEmailAlreadyRegistered
- [ ] UT002 - AuthService_RegisterAsync_ShouldFail_WhenRoleIsAdminOrStaff
- [ ] UT003 - AuthService_RegisterAsync_ShouldFail_WhenCreateUserReturnsErrors
- [ ] UT004 - AuthService_RegisterAsync_ShouldSendEmailConfirmation_WhenRegistrationSucceeds
- [ ] UT005 - AuthService_RegisterAsync_ShouldReturnUserIdAndEmail_WhenRegistrationSucceeds
- [ ] UT006 - AuthService_ConfirmEmailAsync_ShouldFail_WhenUserNotFound
- [ ] UT007 - AuthService_ConfirmEmailAsync_ShouldReturnAlreadyConfirmed_WhenEmailAlreadyConfirmed
- [ ] UT008 - AuthService_ConfirmEmailAsync_ShouldFail_WhenTokenInvalid
- [ ] UT009 - AuthService_ConfirmEmailAsync_ShouldSetUserActive_WhenConfirmationSucceeds
- [ ] UT010 - AuthService_ResendConfirmationAsync_ShouldReturnGenericSuccess_WhenUserNotFound
- [ ] UT011 - AuthService_ResendConfirmationAsync_ShouldFail_WhenEmailAlreadyConfirmed
- [ ] UT012 - AuthService_ResendConfirmationAsync_ShouldFail_WhenEmailSendingThrows
- [ ] UT013 - AuthService_ResendConfirmationAsync_ShouldSucceed_WhenEmailSent
- [ ] UT014 - AuthService_LoginAsync_ShouldFail_WhenUserNotFound
- [ ] UT015 - AuthService_LoginAsync_ShouldFail_WhenUserIsBanned
- [ ] UT016 - AuthService_LoginAsync_ShouldFail_WhenEmailNotConfirmed
- [ ] UT017 - AuthService_LoginAsync_ShouldFail_WhenAccountIsLockedOut
- [ ] UT018 - AuthService_LoginAsync_ShouldFail_WhenPasswordInvalid
- [ ] UT019 - AuthService_LoginAsync_ShouldReturnTokens_WhenCredentialsValid
- [ ] UT020 - AuthService_LoginAsync_ShouldPersistRefreshToken_WhenCredentialsValid

- [ ] UT021 - AuthService_RefreshTokenAsync_ShouldFail_WhenTokenNotFound
- [ ] UT022 - AuthService_RefreshTokenAsync_ShouldFail_WhenUserNotFound
- [ ] UT023 - AuthService_RefreshTokenAsync_ShouldFail_WhenTokenInactive
- [ ] UT024 - AuthService_RefreshTokenAsync_ShouldRevokeOldTokenAndCreateNewToken_WhenTokenValid
- [ ] UT025 - AuthService_RevokeTokenAsync_ShouldFail_WhenTokenNotFound
- [ ] UT026 - AuthService_RevokeTokenAsync_ShouldFail_WhenTokenAlreadyInactive
- [ ] UT027 - AuthService_RevokeTokenAsync_ShouldRevokeToken_WhenTokenActive
- [ ] UT028 - AuthService_LogoutAsync_ShouldRevokeAllActiveTokensAndSignOut
- [ ] UT029 - AuthService_ForgotPasswordAsync_ShouldReturnGenericSuccess_WhenEmailEmpty
- [ ] UT030 - AuthService_ForgotPasswordAsync_ShouldFail_WhenEmailSendingThrows
- [ ] UT031 - AuthService_ForgotPasswordAsync_ShouldSendResetEmail_WhenUserExists
- [ ] UT032 - AuthService_ResetPasswordAsync_ShouldFail_WhenUserNotFound
- [ ] UT033 - AuthService_ResetPasswordAsync_ShouldFallbackToRawToken_WhenBase64DecodeFails
- [ ] UT034 - AuthService_ResetPasswordAsync_ShouldRevokeAllActiveTokens_WhenResetSucceeds
- [ ] UT035 - AuthService_ChangePasswordAsync_ShouldFail_WhenUserNotFound
- [ ] UT036 - AuthService_ChangePasswordAsync_ShouldReturnErrors_WhenIdentityChangeFails
- [ ] UT037 - AuthService_ChangePasswordAsync_ShouldRevokeAllActiveTokens_WhenChangeSucceeds

## 2) PaymentService (36 tests)
- [ ] UT038 - PaymentService_GetInvestorPackagesAsync_ShouldReturnOnlyInvestorPackages
- [ ] UT039 - PaymentService_GetStartupPackagesAsync_ShouldReturnOnlyStartupPackages
- [ ] UT040 - PaymentService_CheckoutSubscriptionAsync_ShouldThrow_WhenPackageIdIsNotPositive
- [ ] UT041 - PaymentService_CheckoutSubscriptionAsync_ShouldThrow_WhenPackageNotFound
- [ ] UT042 - PaymentService_CheckoutSubscriptionAsync_ShouldThrow_WhenUserNotFound
- [ ] UT043 - PaymentService_CheckoutSubscriptionAsync_ShouldThrow_WhenPackageRoleMismatch
- [ ] UT044 - PaymentService_CheckoutSubscriptionAsync_ShouldReusePendingTransaction_WhenPendingNotExpired
- [ ] UT045 - PaymentService_CheckoutSubscriptionAsync_ShouldFailOldPendingAndCreateNew_WhenPendingExpired
- [ ] UT046 - PaymentService_CheckoutSubscriptionAsync_ShouldGeneratePaymentCodeWithPrefix_WhenCreated
- [ ] UT047 - PaymentService_CheckoutSubscriptionAsync_ShouldReturnQrCodeUrl_WhenCreated
- [ ] UT048 - PaymentService_CheckoutBookingAsync_ShouldThrow_WhenBookingIdIsNotPositive
- [ ] UT049 - PaymentService_CheckoutBookingAsync_ShouldThrow_WhenPayableBookingNotFound
- [ ] UT050 - PaymentService_CheckoutBookingAsync_ShouldReusePendingTransaction_WhenPendingNotExpired
- [ ] UT051 - PaymentService_CheckoutBookingAsync_ShouldCreatePendingTransaction_WhenNoPendingExists
- [ ] UT052 - PaymentService_UpdatePackageAsync_ShouldThrow_WhenPackageIdIsNotPositive
- [ ] UT053 - PaymentService_UpdatePackageAsync_ShouldThrow_WhenPackageNotFound
- [ ] UT054 - PaymentService_UpdatePackageAsync_ShouldThrow_WhenPackageTargetRoleNotSupported
- [ ] UT055 - PaymentService_UpdatePackageAsync_ShouldThrow_WhenPriceIsNotPositive
- [ ] UT056 - PaymentService_UpdatePackageAsync_ShouldThrow_WhenDurationMonthsIsNotPositive
- [ ] UT057 - PaymentService_UpdatePackageAsync_ShouldThrow_WhenPackageNameIsEmpty
- [ ] UT058 - PaymentService_UpdatePackageAsync_ShouldPersistFields_WhenInputValid
- [ ] UT059 - PaymentService_GetTransactionStatusAsync_ShouldThrow_WhenTransactionNotFound
- [ ] UT060 - PaymentService_GetTransactionStatusAsync_ShouldAutoFailPending_WhenExpired
- [ ] UT061 - PaymentService_GetTransactionStatusAsync_ShouldReturnMappedStatus_WhenFound
- [ ] UT062 - PaymentService_GetBookingPaymentStatusAsync_ShouldThrow_WhenBookingNotFound
- [ ] UT063 - PaymentService_GetBookingPaymentStatusAsync_ShouldThrow_WhenRequesterIsNotCustomer
- [ ] UT064 - PaymentService_GetBookingPaymentStatusAsync_ShouldAutoFailLatestPending_WhenExpired
- [ ] UT065 - PaymentService_GetBookingPaymentTransactionsAsync_ShouldReturnOnlyBookingReferenceTransactions
- [ ] UT066 - PaymentService_ProcessSePayWebhookAsync_ShouldThrow_WhenPaymentCodeNotFoundInPayload
- [ ] UT067 - PaymentService_ProcessSePayWebhookAsync_ShouldReturnIdempotent_WhenTransactionAlreadyCompleted
- [ ] UT068 - PaymentService_ProcessSePayWebhookAsync_ShouldThrow_WhenPendingTransactionNotFound
- [ ] UT069 - PaymentService_ProcessSePayWebhookAsync_ShouldThrow_WhenTransferAmountInsufficient
- [ ] UT070 - PaymentService_ProcessSePayWebhookAsync_ShouldActivateSubscription_WhenReferenceTypeSubscription
- [ ] UT071 - PaymentService_ProcessSePayWebhookAsync_ShouldSetUserPremium_WhenSubscriptionActivated
- [ ] UT072 - PaymentService_ProcessSePayWebhookAsync_ShouldConfirmBookingAndNotifyBothSides_WhenReferenceTypeBooking
- [ ] UT073 - PaymentService_ProcessSePayWebhookAsync_ShouldPersistSePayFieldsAndCompletedAt_WhenProcessed

## 3) DealService (34 tests)
- [ ] UT074 - DealService_CreateDealAsync_ShouldThrow_WhenProjectIdIsNotPositive
- [ ] UT075 - DealService_CreateDealAsync_ShouldThrow_WhenInvestorNotFound
- [ ] UT076 - DealService_CreateDealAsync_ShouldThrow_WhenProjectNotFound
- [ ] UT077 - DealService_CreateDealAsync_ShouldThrow_WhenBlockingDealExists
- [ ] UT078 - DealService_CreateDealAsync_ShouldSetPendingFlags_WhenCreated
- [ ] UT079 - DealService_CreateDealAsync_ShouldSendNotificationToStartup_WhenCreated
- [ ] UT080 - DealService_GetDealsAsync_ShouldReturnPagedDeals
- [ ] UT081 - DealService_GetInvestorDealsAsync_ShouldFilterByInvestorId
- [ ] UT082 - DealService_GetStartupDealsAsync_ShouldFilterByStartupId
- [ ] UT083 - DealService_RespondDealAsync_ShouldThrow_WhenDealNotFound
- [ ] UT084 - DealService_RespondDealAsync_ShouldThrowForbidden_WhenStartupDoesNotOwnProject
- [ ] UT085 - DealService_RespondDealAsync_ShouldThrow_WhenDealStatusIsNotPending
- [ ] UT086 - DealService_RespondDealAsync_ShouldSetConfirmed_WhenAccepted
- [ ] UT087 - DealService_RespondDealAsync_ShouldSetRejected_WhenRejected
- [ ] UT088 - DealService_RespondDealAsync_ShouldNotifyInvestor_WhenResponded
- [ ] UT089 - DealService_GetContractPreviewForInvestorAsync_ShouldThrowForbidden_WhenInvestorDoesNotOwnDeal
- [ ] UT090 - DealService_GetContractPreviewForInvestorAsync_ShouldThrow_WhenStatusNotInSigningFlow
- [ ] UT091 - DealService_GetContractPreviewForInvestorAsync_ShouldReturnHtml_WhenValid
- [ ] UT092 - DealService_InvestorSignContractAsync_ShouldThrow_WhenFinalAmountIsNotPositive
- [ ] UT093 - DealService_InvestorSignContractAsync_ShouldThrow_WhenFinalEquityPercentageIsNegative
- [ ] UT094 - DealService_InvestorSignContractAsync_ShouldThrow_WhenDealStatusIsNotConfirmed
- [ ] UT095 - DealService_InvestorSignContractAsync_ShouldSetWaitingForStartupSignature_WhenSuccessful
- [ ] UT096 - DealService_StartupSignContractAsync_ShouldThrow_WhenDealStatusIsNotWaitingForStartupSignature
- [ ] UT097 - DealService_StartupSignContractAsync_ShouldThrow_WhenInvestorSignatureMissing
- [ ] UT098 - DealService_StartupSignContractAsync_ShouldThrow_WhenPdfGenerationFails
- [ ] UT099 - DealService_StartupSignContractAsync_ShouldThrow_WhenPdfUploadFails
- [ ] UT100 - DealService_StartupSignContractAsync_ShouldFallbackDirectBlockchainCall_WhenQueueFails
- [ ] UT101 - DealService_StartupSignContractAsync_ShouldSetContractSignedAndNotifyInvestor_WhenSuccessful
- [ ] UT102 - DealService_StartupRejectContractAsync_ShouldThrow_WhenDealStatusIsNotWaitingForStartupSignature
- [ ] UT103 - DealService_StartupRejectContractAsync_ShouldSetRejectedAndClearSignatures_WhenSuccessful
- [ ] UT104 - DealService_GetContractStatusForInvestorAsync_ShouldThrowForbidden_WhenInvestorDoesNotOwnDeal
- [ ] UT105 - DealService_GetOwnershipAssignmentStatusAsync_ShouldThrow_WhenNoRegisteredProjectDocumentExists
- [ ] UT106 - DealService_GetOwnershipAssignmentStatusAsync_ShouldReturnAssigned_WhenInvestorWalletExistsOnChain
- [ ] UT107 - DealService_GetOwnershipAssignmentStatusAsync_ShouldReturnUnassigned_WhenInvestorWalletMissingOnChain

## 4) BookingService (22 tests)
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
- [ ] UT123 - BookingService_ApproveBookingAsync_ShouldThrow_WhenBookingNotFound
- [ ] UT124 - BookingService_ApproveBookingAsync_ShouldThrow_WhenAdvisorResponseWindowExpired
- [ ] UT125 - BookingService_ApproveBookingAsync_ShouldSetConfirmed_WhenPaymentWaivedOrPriceZero
- [ ] UT126 - BookingService_ApproveBookingAsync_ShouldSetApprovedAwaitingPayment_WhenPaymentRequired
- [ ] UT127 - BookingService_RejectBookingAsync_ShouldThrow_WhenStatusCannotBeRejected
- [ ] UT128 - BookingService_RejectBookingAsync_ShouldReleaseSlotsAndNotifyCustomer_WhenRejected
- [ ] UT129 - BookingService_ExpirePendingAdvisorResponsesAsync_ShouldMarkNoResponseAndNotifyCustomer

## 5) ConsultingReportService (19 tests)
- [ ] UT130 - ConsultingReportService_CreateAsync_ShouldThrowForbidden_WhenCurrentUserIsNotAdvisor
- [ ] UT131 - ConsultingReportService_CreateAsync_ShouldThrow_WhenBookingNotFound
- [ ] UT132 - ConsultingReportService_CreateAsync_ShouldThrowForbidden_WhenAdvisorNotAssignedToBooking
- [ ] UT133 - ConsultingReportService_CreateAsync_ShouldThrow_WhenBookingStatusIsNotConfirmed
- [ ] UT134 - ConsultingReportService_CreateAsync_ShouldThrow_WhenSubmissionWindowExpired
- [ ] UT135 - ConsultingReportService_CreateAsync_ShouldCreateSubmittedReport_WhenNoExistingReport
- [ ] UT136 - ConsultingReportService_CreateAsync_ShouldThrow_WhenExistingReportIsNotRevisionRequested
- [ ] UT137 - ConsultingReportService_CreateAsync_ShouldUpdateReport_WhenRevisionRequestedAndWithinDeadline
- [ ] UT138 - ConsultingReportService_ApproveAsync_ShouldThrow_WhenReportStatusIsNotSubmitted
- [ ] UT139 - ConsultingReportService_ApproveAsync_ShouldCompleteBookingAndCloseChat_WhenApproved
- [ ] UT140 - ConsultingReportService_ApproveAsync_ShouldDisburseAdvisorPayout_WhenApproved
- [ ] UT141 - ConsultingReportService_RequestRevisionAsync_ShouldThrow_WhenReportStatusIsNotSubmitted
- [ ] UT142 - ConsultingReportService_RequestRevisionAsync_ShouldEscalateToStaff_WhenRevisionCountReachedMax
- [ ] UT143 - ConsultingReportService_RequestRevisionAsync_ShouldSetRevisionRequestedAndAdvisorDue_WhenUnderLimit
- [ ] UT144 - ConsultingReportService_AcceptComplaintByStaffAsync_ShouldThrow_WhenReportStatusIsNotEscalated
- [ ] UT145 - ConsultingReportService_AcceptComplaintByStaffAsync_ShouldSkipPayoutAndRefundQuota_WhenAccepted
- [ ] UT146 - ConsultingReportService_RejectComplaintByStaffAsync_ShouldDisbursePayout_WhenRejected
- [ ] UT147 - ConsultingReportService_ProcessReportDeadlinesAsync_ShouldAutoApprove_WhenStartupReviewTimesOut
- [ ] UT148 - ConsultingReportService_ProcessReportDeadlinesAsync_ShouldEscalateToStaff_WhenAdvisorRevisionTimesOut

## 6) ProjectService (15 tests)
- [ ] UT149 - ProjectService_GetProjectByIdAsync_ShouldThrow_WhenProjectNotFound
- [ ] UT150 - ProjectService_GetProjectByIdAsync_ShouldBypassQuota_WhenRoleIsStaffOrAdmin
- [ ] UT151 - ProjectService_GetProjectByIdAsync_ShouldBypassQuota_WhenStartupOwnsProject
- [ ] UT152 - ProjectService_GetProjectByIdAsync_ShouldThrow_WhenNoActiveSubscriptionForQuotaRoles
- [ ] UT153 - ProjectService_GetProjectByIdAsync_ShouldThrow_WhenProjectViewQuotaExceeded
- [ ] UT154 - ProjectService_GetProjectByIdAsync_ShouldConsumeQuotaAndUnlockProject_WhenFirstView
- [ ] UT155 - ProjectService_CreateProjectAsync_ShouldThrow_WhenStartupProfileNotFound
- [ ] UT156 - ProjectService_CreateProjectAsync_ShouldSetDraftAndUploadImage_WhenValidRequest
- [ ] UT157 - ProjectService_UpdateProjectAsync_ShouldThrow_WhenProjectNotFound
- [ ] UT158 - ProjectService_UpdateProjectAsync_ShouldThrowForbidden_WhenStartupDoesNotOwnProject
- [ ] UT159 - ProjectService_UpdateProjectAsync_ShouldThrow_WhenStatusIsNotDraftOrRejected
- [ ] UT160 - ProjectService_UpdateProjectAsync_ShouldMoveRejectedToDraft_BeforeApplyingUpdates
- [ ] UT161 - ProjectService_SubmitProjectAsync_ShouldThrow_WhenProjectStatusIsNotDraft
- [ ] UT162 - ProjectService_RejectProjectAsync_ShouldThrow_WhenProjectStatusIsNotPending
- [ ] UT163 - ProjectService_RejectProjectAsync_ShouldSetRejectedMetadata_WhenSuccessful

## 7) DocumentService (18 tests)
- [ ] UT164 - DocumentService_UploadDocumentAsync_ShouldThrow_WhenProjectNotFound
- [ ] UT165 - DocumentService_UploadDocumentAsync_ShouldThrowUnauthorized_WhenStartupDoesNotOwnProject
- [ ] UT166 - DocumentService_UploadDocumentAsync_ShouldThrow_WhenProjectStatusIsNotDraft
- [ ] UT167 - DocumentService_UploadDocumentAsync_ShouldThrow_WhenDuplicateFileHashExistsInDatabase
- [ ] UT168 - DocumentService_UploadDocumentAsync_ShouldThrow_WhenFileHashAlreadyExistsOnBlockchain
- [ ] UT169 - DocumentService_UploadDocumentAsync_ShouldWrapError_WhenBlockchainVerifyFailsUnexpectedly
- [ ] UT170 - DocumentService_UploadDocumentAsync_ShouldPersistDocumentWithHashAndUrl_WhenValid
- [ ] UT171 - DocumentService_GetByIdAsync_ShouldReturnNull_WhenDocumentNotFound
- [ ] UT172 - DocumentService_GetByIdAsync_ShouldThrowUnauthorized_WhenUserCannotViewProjectDocuments
- [ ] UT173 - DocumentService_DeleteAsync_ShouldReturnFalse_WhenDocumentNotFound
- [ ] UT174 - DocumentService_DeleteAsync_ShouldThrowUnauthorized_WhenStartupCannotDeleteForeignDocument
- [ ] UT175 - DocumentService_DeleteAsync_ShouldThrow_WhenProjectIsLockedByApprovedStatus
- [ ] UT176 - DocumentService_DeleteAsync_ShouldDeleteDocument_WhenAllowed
- [ ] UT177 - DocumentService_VerifyDocumentAsync_ShouldThrow_WhenDocumentNotFound
- [ ] UT178 - DocumentService_VerifyDocumentAsync_ShouldThrow_WhenDocumentNotRegisteredOnBlockchain
- [ ] UT179 - DocumentService_ApproveProjectAsync_ShouldThrow_WhenProjectStatusIsNotPending
- [ ] UT180 - DocumentService_ApproveProjectAsync_ShouldThrow_WhenNoProjectDocumentExists
- [ ] UT181 - DocumentService_ApproveProjectAsync_ShouldRegisterHashAndApproveProject_WhenValid

## 8) ConnectionService (14 tests)
- [ ] UT182 - ConnectionService_GetInvestorRequestsAsync_ShouldThrow_WhenStatusFilterInvalid
- [ ] UT183 - ConnectionService_GetInvestorRequestsAsync_ShouldApplyStatusFilter_WhenProvided
- [ ] UT184 - ConnectionService_GetStartupRequestsAsync_ShouldApplyStatusFilter_WhenProvided
- [ ] UT185 - ConnectionService_CreateRequestAsync_ShouldThrow_WhenInvestorNotFound
- [ ] UT186 - ConnectionService_CreateRequestAsync_ShouldThrow_WhenProjectNotFound
- [ ] UT187 - ConnectionService_CreateRequestAsync_ShouldThrow_WhenPendingRequestAlreadyExists
- [ ] UT188 - ConnectionService_CreateRequestAsync_ShouldPersistPendingRequestAndNotifyStartup_WhenValid
- [ ] UT189 - ConnectionService_RespondToRequestAsync_ShouldThrow_WhenRequestNotFound
- [ ] UT190 - ConnectionService_RespondToRequestAsync_ShouldThrowForbidden_WhenStartupDoesNotOwnProject
- [ ] UT191 - ConnectionService_RespondToRequestAsync_ShouldThrow_WhenRequestStatusIsNotPending
- [ ] UT192 - ConnectionService_RespondToRequestAsync_ShouldOpenChatSession_WhenAccepted
- [ ] UT193 - ConnectionService_RespondToRequestAsync_ShouldThrow_WhenAcceptedButChatSessionNotCreated
- [ ] UT194 - ConnectionService_RespondToRequestAsync_ShouldNotifyInvestorWithConnectionReference_WhenRejected
- [ ] UT195 - ConnectionService_GetFounderContactAsync_ShouldThrowUnauthorized_WhenNoAcceptedConnectionExists

## 9) ChatSessionService (10 tests)
- [ ] UT196 - ChatSessionService_OpenSessionAsync_ShouldReturnNull_WhenBookingNotFound
- [ ] UT197 - ChatSessionService_OpenSessionAsync_ShouldReturnNull_WhenUserNotBookingParticipant
- [ ] UT198 - ChatSessionService_OpenSessionAsync_ShouldReturnNull_WhenBookingStatusIsNotConfirmed
- [ ] UT199 - ChatSessionService_OpenSessionAsync_ShouldReturnExistingSession_WhenAlreadyExists
- [ ] UT200 - ChatSessionService_OpenSessionAsync_ShouldCreateSession_WhenNotExistsAndConfirmed
- [ ] UT201 - ChatSessionService_OpenSessionByConnectionRequestAsync_ShouldReturnNull_WhenRequestInvalidOrNotAccepted
- [ ] UT202 - ChatSessionService_OpenSessionByConnectionRequestAsync_ShouldReturnExistingSession_WhenAlreadyExists
- [ ] UT203 - ChatSessionService_GetSessionAsync_ShouldReturnNull_WhenUserNotParticipant
- [ ] UT204 - ChatSessionService_CloseSessionAsync_ShouldReturnFalse_WhenSessionMissingClosedOrUnauthorized
- [ ] UT205 - ChatSessionService_CloseSessionAsync_ShouldCloseSessionAndSetEndTime_WhenAuthorized

## 10) ChatMessageService (8 tests)
- [ ] UT206 - ChatMessageService_GetMessagesAsync_ShouldReturnEmpty_WhenSessionNotFound
- [ ] UT207 - ChatMessageService_GetMessagesAsync_ShouldReturnEmpty_WhenUserNotParticipant
- [ ] UT208 - ChatMessageService_GetMessagesAsync_ShouldReturnMappedMessages_WhenUserIsParticipant
- [ ] UT209 - ChatMessageService_SendMessageAsync_ShouldReturnNull_WhenSessionNotFound
- [ ] UT210 - ChatMessageService_SendMessageAsync_ShouldReturnNull_WhenSessionClosed
- [ ] UT211 - ChatMessageService_SendMessageAsync_ShouldReturnNull_WhenUserNotParticipant
- [ ] UT212 - ChatMessageService_SendMessageAsync_ShouldAutoCloseSessionAndReturnNull_WhenBookingCompleted
- [ ] UT213 - ChatMessageService_SendMessageAsync_ShouldPersistAndReturnMessage_WhenValid

## 11) NotificationService (8 tests)
- [ ] UT214 - NotificationService_SendNotificationAsync_ShouldPersistNotification_BeforeRealtimePublish
- [ ] UT215 - NotificationService_SendNotificationAsync_ShouldNotThrow_WhenRealtimePublishFails
- [ ] UT216 - NotificationService_GetUserNotificationsAsync_ShouldApplyDefaultPagination_WhenModelIsEmpty
- [ ] UT217 - NotificationService_GetUserNotificationsAsync_ShouldCapPageSizeTo100_WhenRequestedTooLarge
- [ ] UT218 - NotificationService_MarkAsReadAsync_ShouldReturnFalse_WhenRepositoryReturnsFalse
- [ ] UT219 - NotificationService_MarkAsReadAsync_ShouldSaveChangesAndReturnTrue_WhenRepositoryReturnsTrue
- [ ] UT220 - NotificationService_MarkAllAsReadAsync_ShouldSaveChangesAndReturnTrue
- [ ] UT221 - NotificationService_DeleteNotificationAsync_ShouldFollowRepositoryResult_AndSaveOnSuccess

## 12) BlockchainService (10 tests)
- [ ] UT222 - BlockchainService_ComputeFileHashAsync_ShouldReturnHexSha256_With0xPrefix
- [ ] UT223 - BlockchainService_ComputeFileHashFromUrlAsync_ShouldReturnHexSha256_With0xPrefix
- [ ] UT224 - BlockchainService_RegisterDocumentAsync_ShouldThrow_WhenTransactionReverted
- [ ] UT225 - BlockchainService_RegisterDocumentAsync_ShouldReturnTransactionHash_WhenSuccessful
- [ ] UT226 - BlockchainService_AssignDocumentOwnerAsync_ShouldThrow_WhenFileHashEmpty
- [ ] UT227 - BlockchainService_AssignDocumentOwnerAsync_ShouldThrow_WhenInvestorWalletEmpty
- [ ] UT228 - BlockchainService_AssignDocumentOwnerAsync_ShouldWrapRevertException_AsInvalidOperation
- [ ] UT229 - BlockchainService_VerifyDocumentAsync_ShouldReturnEmptyTuple_WhenHashNotFoundOnChain
- [ ] UT230 - BlockchainService_VerifyDocumentAsync_ShouldReturnStartupTimestampAndOwners_WhenFound
- [ ] UT231 - BlockchainService_VerifyProjectDocumentsAsync_ShouldAggregateVerifiedAndUnverifiedDocuments

## 13) Background Services + Middleware (13 tests)
- [ ] UT232 - GlobalExceptionMiddleware_ShouldMapValidationException_To400BadRequest
- [ ] UT233 - GlobalExceptionMiddleware_ShouldMapKeyNotFoundException_To404NotFound
- [ ] UT234 - GlobalExceptionMiddleware_ShouldMapForbiddenAccessException_To403Forbidden
- [ ] UT235 - GlobalExceptionMiddleware_ShouldMapInvalidOperationException_To409Conflict
- [ ] UT236 - GlobalExceptionMiddleware_ShouldMapHttpRequestException_To502BadGateway
- [ ] UT237 - GlobalExceptionMiddleware_ShouldMapUnknownException_To500InternalServerError
- [ ] UT238 - GlobalExceptionMiddleware_ShouldSkipWritingResponse_WhenResponseHasStarted
- [ ] UT239 - BlockchainOwnershipAssignmentBackgroundService_ShouldAssignOwnerAndNotify_WhenWorkItemDequeued
- [ ] UT240 - BlockchainOwnershipAssignmentBackgroundService_ShouldLogErrorAndContinue_WhenAssignmentFails
- [ ] UT241 - BookingResponseExpiryBackgroundService_ShouldInvokeExpirePendingAdvisorResponses_PerCycle
- [ ] UT242 - ConsultingReportDeadlineBackgroundService_ShouldInvokeProcessReportDeadlines_PerCycle
- [ ] UT243 - SubscriptionExpiryBackgroundService_ShouldMarkExpiredAndRevokePremium_WhenNoActiveSubscriptionLeft
- [ ] UT244 - ProjectAdvisorAutoAssignBackgroundService_ShouldInvokeAutoAssignUnassignedApprovedProjects_PerCycle

## 14) Critical API Controllers (20 tests)
- [ ] UT245 - AuthController_Register_ShouldReturnBadRequest_WhenModelStateInvalid
- [ ] UT246 - AuthController_Register_ShouldReturnBadRequest_WhenPasswordAndConfirmPasswordMismatch
- [ ] UT247 - AuthController_Register_ShouldReturnBadRequest_WhenServiceReturnsFailure
- [ ] UT248 - AuthController_Register_ShouldReturnOkWithUserInfo_WhenServiceSucceeds
- [ ] UT249 - AuthController_Login_ShouldReturnUnauthorized_WhenServiceReturnsFailure
- [ ] UT250 - AuthController_Login_ShouldReturnOk_WhenServiceSucceeds
- [ ] UT251 - PaymentController_CheckoutBooking_ShouldReturn404_WhenServiceThrowsKeyNotFound
- [ ] UT252 - PaymentController_CheckoutBooking_ShouldReturn400_WhenServiceThrowsInvalidOperation
- [ ] UT253 - PaymentController_CheckoutSubscription_ShouldReturn404_WhenServiceThrowsKeyNotFound
- [ ] UT254 - PaymentController_SePayWebhook_ShouldReturn400_WhenServiceThrowsInvalidOperation
- [ ] UT255 - DealsController_GetDeals_ShouldCallGetInvestorDeals_WhenCurrentRoleIsInvestor
- [ ] UT256 - DealsController_GetDeals_ShouldCallGetStartupDeals_WhenCurrentRoleIsStartup
- [ ] UT257 - DealsController_GetDeals_ShouldCallGetDeals_WhenCurrentRoleIsStaffOrAdmin
- [ ] UT258 - DealsController_RespondDeal_ShouldThrow_WhenIsAcceptedMissing
- [ ] UT259 - BookingController_CreateBooking_ShouldReturnCreated_WhenServiceReturnsBooking
- [ ] UT260 - BookingController_CreateBooking_ShouldReturnBadRequest_WhenServiceReturnsNull
- [ ] UT261 - ConsultingReportController_GetById_ShouldReturn404_WhenReportMissing
- [ ] UT262 - ProjectsController_VerifyBlockchain_ShouldReturn400_WhenNotFullyVerified
- [ ] UT263 - DocumentController_Upload_ShouldReturn409_WhenServiceThrowsInvalidOperation
- [ ] UT264 - DocumentController_VerifyDocument_ShouldReturn502_WhenServiceThrowsHttpRequestException

## 15) Core Validators (14 tests)
- [ ] UT265 - RegisterRequestValidator_ShouldPass_WhenRequestValid
- [ ] UT266 - RegisterRequestValidator_ShouldFail_WhenPasswordNotComplex
- [ ] UT267 - InvestorSignContractDtoValidator_ShouldPass_WhenRequestValid
- [ ] UT268 - InvestorSignContractDtoValidator_ShouldFail_WhenSignatureMissing
- [ ] UT269 - CreateBookingRequestValidator_ShouldPass_WhenSlotIdsUniqueAndValid
- [ ] UT270 - CreateBookingRequestValidator_ShouldFail_WhenSlotIdsContainDuplicates
- [ ] UT271 - CreateProjectRequestValidator_ShouldPass_WhenGrowthFieldsComplete
- [ ] UT272 - CreateProjectRequestValidator_ShouldFail_WhenGrowthRevenueMissingOrNonPositive
- [ ] UT273 - UploadDocumentRequestValidator_ShouldPass_WhenPdfWithinSizeLimit
- [ ] UT274 - UploadDocumentRequestValidator_ShouldFail_WhenMimeTypeNotAllowed
- [ ] UT275 - CreateConsultingReportRequestValidator_ShouldPass_WhenRequiredFieldsProvided
- [ ] UT276 - CreateConsultingReportRequestValidator_ShouldFail_WhenRequiredFieldsMissing
- [ ] UT277 - SendMessageRequestValidator_ShouldPass_WhenContentValid
- [ ] UT278 - SendMessageRequestValidator_ShouldFail_WhenContentEmpty

## Completion Notes
- Add actual test class path beside each item during implementation.
- If one test is intentionally skipped, document reason in PR.

