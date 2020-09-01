#include <memory>
#include "vulkan_binding_source.cpp"

extern "C" void __c__vkCreateInstance(VkResult *ret, const VkInstanceCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkInstance * _pInstance) {
    *ret = (VkResult )vkCreateInstance(_pCreateInfo, _pAllocator, _pInstance);
}
extern "C" void __c__vkDestroyInstance(VkInstance_T * _instance, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyInstance(_instance, _pAllocator);
}
extern "C" void __c__vkEnumeratePhysicalDevices(VkResult *ret, VkInstance_T * _instance, uint32_t * _pPhysicalDeviceCount, VkPhysicalDevice * _pPhysicalDevices) {
    *ret = (VkResult )vkEnumeratePhysicalDevices(_instance, _pPhysicalDeviceCount, _pPhysicalDevices);
}
extern "C" void __c__vkGetPhysicalDeviceFeatures(VkPhysicalDevice_T * _physicalDevice, VkPhysicalDeviceFeatures * _pFeatures) {
    vkGetPhysicalDeviceFeatures(_physicalDevice, _pFeatures);
}
extern "C" void __c__vkGetPhysicalDeviceFormatProperties(VkPhysicalDevice_T * _physicalDevice, VkFormat _format, VkFormatProperties * _pFormatProperties) {
    vkGetPhysicalDeviceFormatProperties(_physicalDevice, _format, _pFormatProperties);
}
extern "C" void __c__vkGetPhysicalDeviceImageFormatProperties(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, VkFormat _format, VkImageType _type, VkImageTiling _tiling, uint32_t _usage, uint32_t _flags, VkImageFormatProperties * _pImageFormatProperties) {
    *ret = (VkResult )vkGetPhysicalDeviceImageFormatProperties(_physicalDevice, _format, _type, _tiling, _usage, _flags, _pImageFormatProperties);
}
extern "C" void __c__vkGetPhysicalDeviceProperties(VkPhysicalDevice_T * _physicalDevice, VkPhysicalDeviceProperties * _pProperties) {
    vkGetPhysicalDeviceProperties(_physicalDevice, _pProperties);
}
extern "C" void __c__vkGetPhysicalDeviceQueueFamilyProperties(VkPhysicalDevice_T * _physicalDevice, uint32_t * _pQueueFamilyPropertyCount, VkQueueFamilyProperties * _pQueueFamilyProperties) {
    vkGetPhysicalDeviceQueueFamilyProperties(_physicalDevice, _pQueueFamilyPropertyCount, _pQueueFamilyProperties);
}
extern "C" void __c__vkGetPhysicalDeviceMemoryProperties(VkPhysicalDevice_T * _physicalDevice, VkPhysicalDeviceMemoryProperties * _pMemoryProperties) {
    vkGetPhysicalDeviceMemoryProperties(_physicalDevice, _pMemoryProperties);
}
extern "C" void __c__vkGetInstanceProcAddr(PFN_vkVoidFunction *ret, VkInstance_T * _instance, char * _pName) {
    *ret = (PFN_vkVoidFunction )vkGetInstanceProcAddr(_instance, _pName);
}
extern "C" void __c__vkGetDeviceProcAddr(PFN_vkVoidFunction *ret, VkDevice_T * _device, char * _pName) {
    *ret = (PFN_vkVoidFunction )vkGetDeviceProcAddr(_device, _pName);
}
extern "C" void __c__vkCreateDevice(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, const VkDeviceCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkDevice * _pDevice) {
    *ret = (VkResult )vkCreateDevice(_physicalDevice, _pCreateInfo, _pAllocator, _pDevice);
}
extern "C" void __c__vkDestroyDevice(VkDevice_T * _device, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyDevice(_device, _pAllocator);
}
extern "C" void __c__vkEnumerateInstanceExtensionProperties(VkResult *ret, char * _pLayerName, uint32_t * _pPropertyCount, VkExtensionProperties * _pProperties) {
    *ret = (VkResult )vkEnumerateInstanceExtensionProperties(_pLayerName, _pPropertyCount, _pProperties);
}
extern "C" void __c__vkEnumerateDeviceExtensionProperties(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, char * _pLayerName, uint32_t * _pPropertyCount, VkExtensionProperties * _pProperties) {
    *ret = (VkResult )vkEnumerateDeviceExtensionProperties(_physicalDevice, _pLayerName, _pPropertyCount, _pProperties);
}
extern "C" void __c__vkEnumerateInstanceLayerProperties(VkResult *ret, uint32_t * _pPropertyCount, VkLayerProperties * _pProperties) {
    *ret = (VkResult )vkEnumerateInstanceLayerProperties(_pPropertyCount, _pProperties);
}
extern "C" void __c__vkEnumerateDeviceLayerProperties(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, uint32_t * _pPropertyCount, VkLayerProperties * _pProperties) {
    *ret = (VkResult )vkEnumerateDeviceLayerProperties(_physicalDevice, _pPropertyCount, _pProperties);
}
extern "C" void __c__vkGetDeviceQueue(VkDevice_T * _device, uint32_t _queueFamilyIndex, uint32_t _queueIndex, VkQueue * _pQueue) {
    vkGetDeviceQueue(_device, _queueFamilyIndex, _queueIndex, _pQueue);
}
extern "C" void __c__vkQueueSubmit(VkResult *ret, VkQueue_T * _queue, uint32_t _submitCount, const VkSubmitInfo * _pSubmits, VkFence_T * _fence) {
    *ret = (VkResult )vkQueueSubmit(_queue, _submitCount, _pSubmits, _fence);
}
extern "C" void __c__vkQueueWaitIdle(VkResult *ret, VkQueue_T * _queue) {
    *ret = (VkResult )vkQueueWaitIdle(_queue);
}
extern "C" void __c__vkDeviceWaitIdle(VkResult *ret, VkDevice_T * _device) {
    *ret = (VkResult )vkDeviceWaitIdle(_device);
}
extern "C" void __c__vkAllocateMemory(VkResult *ret, VkDevice_T * _device, const VkMemoryAllocateInfo * _pAllocateInfo, const VkAllocationCallbacks * _pAllocator, VkDeviceMemory * _pMemory) {
    *ret = (VkResult )vkAllocateMemory(_device, _pAllocateInfo, _pAllocator, _pMemory);
}
extern "C" void __c__vkFreeMemory(VkDevice_T * _device, VkDeviceMemory_T * _memory, const VkAllocationCallbacks * _pAllocator) {
    vkFreeMemory(_device, _memory, _pAllocator);
}
extern "C" void __c__vkMapMemory(VkResult *ret, VkDevice_T * _device, VkDeviceMemory_T * _memory, uint64_t _offset, uint64_t _size, uint32_t _flags, void * * _ppData) {
    *ret = (VkResult )vkMapMemory(_device, _memory, _offset, _size, _flags, _ppData);
}
extern "C" void __c__vkUnmapMemory(VkDevice_T * _device, VkDeviceMemory_T * _memory) {
    vkUnmapMemory(_device, _memory);
}
extern "C" void __c__vkFlushMappedMemoryRanges(VkResult *ret, VkDevice_T * _device, uint32_t _memoryRangeCount, const VkMappedMemoryRange * _pMemoryRanges) {
    *ret = (VkResult )vkFlushMappedMemoryRanges(_device, _memoryRangeCount, _pMemoryRanges);
}
extern "C" void __c__vkInvalidateMappedMemoryRanges(VkResult *ret, VkDevice_T * _device, uint32_t _memoryRangeCount, const VkMappedMemoryRange * _pMemoryRanges) {
    *ret = (VkResult )vkInvalidateMappedMemoryRanges(_device, _memoryRangeCount, _pMemoryRanges);
}
extern "C" void __c__vkGetDeviceMemoryCommitment(VkDevice_T * _device, VkDeviceMemory_T * _memory, VkDeviceSize * _pCommittedMemoryInBytes) {
    vkGetDeviceMemoryCommitment(_device, _memory, _pCommittedMemoryInBytes);
}
extern "C" void __c__vkBindBufferMemory(VkResult *ret, VkDevice_T * _device, VkBuffer_T * _buffer, VkDeviceMemory_T * _memory, uint64_t _memoryOffset) {
    *ret = (VkResult )vkBindBufferMemory(_device, _buffer, _memory, _memoryOffset);
}
extern "C" void __c__vkBindImageMemory(VkResult *ret, VkDevice_T * _device, VkImage_T * _image, VkDeviceMemory_T * _memory, uint64_t _memoryOffset) {
    *ret = (VkResult )vkBindImageMemory(_device, _image, _memory, _memoryOffset);
}
extern "C" void __c__vkGetBufferMemoryRequirements(VkDevice_T * _device, VkBuffer_T * _buffer, VkMemoryRequirements * _pMemoryRequirements) {
    vkGetBufferMemoryRequirements(_device, _buffer, _pMemoryRequirements);
}
extern "C" void __c__vkGetImageMemoryRequirements(VkDevice_T * _device, VkImage_T * _image, VkMemoryRequirements * _pMemoryRequirements) {
    vkGetImageMemoryRequirements(_device, _image, _pMemoryRequirements);
}
extern "C" void __c__vkGetImageSparseMemoryRequirements(VkDevice_T * _device, VkImage_T * _image, uint32_t * _pSparseMemoryRequirementCount, VkSparseImageMemoryRequirements * _pSparseMemoryRequirements) {
    vkGetImageSparseMemoryRequirements(_device, _image, _pSparseMemoryRequirementCount, _pSparseMemoryRequirements);
}
extern "C" void __c__vkGetPhysicalDeviceSparseImageFormatProperties(VkPhysicalDevice_T * _physicalDevice, VkFormat _format, VkImageType _type, VkSampleCountFlagBits _samples, uint32_t _usage, VkImageTiling _tiling, uint32_t * _pPropertyCount, VkSparseImageFormatProperties * _pProperties) {
    vkGetPhysicalDeviceSparseImageFormatProperties(_physicalDevice, _format, _type, _samples, _usage, _tiling, _pPropertyCount, _pProperties);
}
extern "C" void __c__vkQueueBindSparse(VkResult *ret, VkQueue_T * _queue, uint32_t _bindInfoCount, const VkBindSparseInfo * _pBindInfo, VkFence_T * _fence) {
    *ret = (VkResult )vkQueueBindSparse(_queue, _bindInfoCount, _pBindInfo, _fence);
}
extern "C" void __c__vkCreateFence(VkResult *ret, VkDevice_T * _device, const VkFenceCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkFence * _pFence) {
    *ret = (VkResult )vkCreateFence(_device, _pCreateInfo, _pAllocator, _pFence);
}
extern "C" void __c__vkDestroyFence(VkDevice_T * _device, VkFence_T * _fence, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyFence(_device, _fence, _pAllocator);
}
extern "C" void __c__vkResetFences(VkResult *ret, VkDevice_T * _device, uint32_t _fenceCount, const VkFence * _pFences) {
    *ret = (VkResult )vkResetFences(_device, _fenceCount, _pFences);
}
extern "C" void __c__vkGetFenceStatus(VkResult *ret, VkDevice_T * _device, VkFence_T * _fence) {
    *ret = (VkResult )vkGetFenceStatus(_device, _fence);
}
extern "C" void __c__vkWaitForFences(VkResult *ret, VkDevice_T * _device, uint32_t _fenceCount, const VkFence * _pFences, uint32_t _waitAll, uint64_t _timeout) {
    *ret = (VkResult )vkWaitForFences(_device, _fenceCount, _pFences, _waitAll, _timeout);
}
extern "C" void __c__vkCreateSemaphore(VkResult *ret, VkDevice_T * _device, const VkSemaphoreCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkSemaphore * _pSemaphore) {
    *ret = (VkResult )vkCreateSemaphore(_device, _pCreateInfo, _pAllocator, _pSemaphore);
}
extern "C" void __c__vkDestroySemaphore(VkDevice_T * _device, VkSemaphore_T * _semaphore, const VkAllocationCallbacks * _pAllocator) {
    vkDestroySemaphore(_device, _semaphore, _pAllocator);
}
extern "C" void __c__vkCreateEvent(VkResult *ret, VkDevice_T * _device, const VkEventCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkEvent * _pEvent) {
    *ret = (VkResult )vkCreateEvent(_device, _pCreateInfo, _pAllocator, _pEvent);
}
extern "C" void __c__vkDestroyEvent(VkDevice_T * _device, VkEvent_T * _event, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyEvent(_device, _event, _pAllocator);
}
extern "C" void __c__vkGetEventStatus(VkResult *ret, VkDevice_T * _device, VkEvent_T * _event) {
    *ret = (VkResult )vkGetEventStatus(_device, _event);
}
extern "C" void __c__vkSetEvent(VkResult *ret, VkDevice_T * _device, VkEvent_T * _event) {
    *ret = (VkResult )vkSetEvent(_device, _event);
}
extern "C" void __c__vkResetEvent(VkResult *ret, VkDevice_T * _device, VkEvent_T * _event) {
    *ret = (VkResult )vkResetEvent(_device, _event);
}
extern "C" void __c__vkCreateQueryPool(VkResult *ret, VkDevice_T * _device, const VkQueryPoolCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkQueryPool * _pQueryPool) {
    *ret = (VkResult )vkCreateQueryPool(_device, _pCreateInfo, _pAllocator, _pQueryPool);
}
extern "C" void __c__vkDestroyQueryPool(VkDevice_T * _device, VkQueryPool_T * _queryPool, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyQueryPool(_device, _queryPool, _pAllocator);
}
extern "C" void __c__vkGetQueryPoolResults(VkResult *ret, VkDevice_T * _device, VkQueryPool_T * _queryPool, uint32_t _firstQuery, uint32_t _queryCount, uint64_t _dataSize, void * _pData, uint64_t _stride, uint32_t _flags) {
    *ret = (VkResult )vkGetQueryPoolResults(_device, _queryPool, _firstQuery, _queryCount, _dataSize, _pData, _stride, _flags);
}
extern "C" void __c__vkCreateBuffer(VkResult *ret, VkDevice_T * _device, const VkBufferCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkBuffer * _pBuffer) {
    *ret = (VkResult )vkCreateBuffer(_device, _pCreateInfo, _pAllocator, _pBuffer);
}
extern "C" void __c__vkDestroyBuffer(VkDevice_T * _device, VkBuffer_T * _buffer, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyBuffer(_device, _buffer, _pAllocator);
}
extern "C" void __c__vkCreateBufferView(VkResult *ret, VkDevice_T * _device, const VkBufferViewCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkBufferView * _pView) {
    *ret = (VkResult )vkCreateBufferView(_device, _pCreateInfo, _pAllocator, _pView);
}
extern "C" void __c__vkDestroyBufferView(VkDevice_T * _device, VkBufferView_T * _bufferView, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyBufferView(_device, _bufferView, _pAllocator);
}
extern "C" void __c__vkCreateImage(VkResult *ret, VkDevice_T * _device, const VkImageCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkImage * _pImage) {
    *ret = (VkResult )vkCreateImage(_device, _pCreateInfo, _pAllocator, _pImage);
}
extern "C" void __c__vkDestroyImage(VkDevice_T * _device, VkImage_T * _image, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyImage(_device, _image, _pAllocator);
}
extern "C" void __c__vkGetImageSubresourceLayout(VkDevice_T * _device, VkImage_T * _image, const VkImageSubresource * _pSubresource, VkSubresourceLayout * _pLayout) {
    vkGetImageSubresourceLayout(_device, _image, _pSubresource, _pLayout);
}
extern "C" void __c__vkCreateImageView(VkResult *ret, VkDevice_T * _device, const VkImageViewCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkImageView * _pView) {
    *ret = (VkResult )vkCreateImageView(_device, _pCreateInfo, _pAllocator, _pView);
}
extern "C" void __c__vkDestroyImageView(VkDevice_T * _device, VkImageView_T * _imageView, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyImageView(_device, _imageView, _pAllocator);
}
extern "C" void __c__vkCreateShaderModule(VkResult *ret, VkDevice_T * _device, const VkShaderModuleCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkShaderModule * _pShaderModule) {
    *ret = (VkResult )vkCreateShaderModule(_device, _pCreateInfo, _pAllocator, _pShaderModule);
}
extern "C" void __c__vkDestroyShaderModule(VkDevice_T * _device, VkShaderModule_T * _shaderModule, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyShaderModule(_device, _shaderModule, _pAllocator);
}
extern "C" void __c__vkCreatePipelineCache(VkResult *ret, VkDevice_T * _device, const VkPipelineCacheCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkPipelineCache * _pPipelineCache) {
    *ret = (VkResult )vkCreatePipelineCache(_device, _pCreateInfo, _pAllocator, _pPipelineCache);
}
extern "C" void __c__vkDestroyPipelineCache(VkDevice_T * _device, VkPipelineCache_T * _pipelineCache, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyPipelineCache(_device, _pipelineCache, _pAllocator);
}
extern "C" void __c__vkGetPipelineCacheData(VkResult *ret, VkDevice_T * _device, VkPipelineCache_T * _pipelineCache, size_t * _pDataSize, void * _pData) {
    *ret = (VkResult )vkGetPipelineCacheData(_device, _pipelineCache, _pDataSize, _pData);
}
extern "C" void __c__vkMergePipelineCaches(VkResult *ret, VkDevice_T * _device, VkPipelineCache_T * _dstCache, uint32_t _srcCacheCount, const VkPipelineCache * _pSrcCaches) {
    *ret = (VkResult )vkMergePipelineCaches(_device, _dstCache, _srcCacheCount, _pSrcCaches);
}
extern "C" void __c__vkCreateGraphicsPipelines(VkResult *ret, VkDevice_T * _device, VkPipelineCache_T * _pipelineCache, uint32_t _createInfoCount, const VkGraphicsPipelineCreateInfo * _pCreateInfos, const VkAllocationCallbacks * _pAllocator, VkPipeline * _pPipelines) {
    *ret = (VkResult )vkCreateGraphicsPipelines(_device, _pipelineCache, _createInfoCount, _pCreateInfos, _pAllocator, _pPipelines);
}
extern "C" void __c__vkCreateComputePipelines(VkResult *ret, VkDevice_T * _device, VkPipelineCache_T * _pipelineCache, uint32_t _createInfoCount, const VkComputePipelineCreateInfo * _pCreateInfos, const VkAllocationCallbacks * _pAllocator, VkPipeline * _pPipelines) {
    *ret = (VkResult )vkCreateComputePipelines(_device, _pipelineCache, _createInfoCount, _pCreateInfos, _pAllocator, _pPipelines);
}
extern "C" void __c__vkDestroyPipeline(VkDevice_T * _device, VkPipeline_T * _pipeline, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyPipeline(_device, _pipeline, _pAllocator);
}
extern "C" void __c__vkCreatePipelineLayout(VkResult *ret, VkDevice_T * _device, const VkPipelineLayoutCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkPipelineLayout * _pPipelineLayout) {
    *ret = (VkResult )vkCreatePipelineLayout(_device, _pCreateInfo, _pAllocator, _pPipelineLayout);
}
extern "C" void __c__vkDestroyPipelineLayout(VkDevice_T * _device, VkPipelineLayout_T * _pipelineLayout, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyPipelineLayout(_device, _pipelineLayout, _pAllocator);
}
extern "C" void __c__vkCreateSampler(VkResult *ret, VkDevice_T * _device, const VkSamplerCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkSampler * _pSampler) {
    *ret = (VkResult )vkCreateSampler(_device, _pCreateInfo, _pAllocator, _pSampler);
}
extern "C" void __c__vkDestroySampler(VkDevice_T * _device, VkSampler_T * _sampler, const VkAllocationCallbacks * _pAllocator) {
    vkDestroySampler(_device, _sampler, _pAllocator);
}
extern "C" void __c__vkCreateDescriptorSetLayout(VkResult *ret, VkDevice_T * _device, const VkDescriptorSetLayoutCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkDescriptorSetLayout * _pSetLayout) {
    *ret = (VkResult )vkCreateDescriptorSetLayout(_device, _pCreateInfo, _pAllocator, _pSetLayout);
}
extern "C" void __c__vkDestroyDescriptorSetLayout(VkDevice_T * _device, VkDescriptorSetLayout_T * _descriptorSetLayout, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyDescriptorSetLayout(_device, _descriptorSetLayout, _pAllocator);
}
extern "C" void __c__vkCreateDescriptorPool(VkResult *ret, VkDevice_T * _device, const VkDescriptorPoolCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkDescriptorPool * _pDescriptorPool) {
    *ret = (VkResult )vkCreateDescriptorPool(_device, _pCreateInfo, _pAllocator, _pDescriptorPool);
}
extern "C" void __c__vkDestroyDescriptorPool(VkDevice_T * _device, VkDescriptorPool_T * _descriptorPool, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyDescriptorPool(_device, _descriptorPool, _pAllocator);
}
extern "C" void __c__vkResetDescriptorPool(VkResult *ret, VkDevice_T * _device, VkDescriptorPool_T * _descriptorPool, uint32_t _flags) {
    *ret = (VkResult )vkResetDescriptorPool(_device, _descriptorPool, _flags);
}
extern "C" void __c__vkAllocateDescriptorSets(VkResult *ret, VkDevice_T * _device, const VkDescriptorSetAllocateInfo * _pAllocateInfo, VkDescriptorSet * _pDescriptorSets) {
    *ret = (VkResult )vkAllocateDescriptorSets(_device, _pAllocateInfo, _pDescriptorSets);
}
extern "C" void __c__vkFreeDescriptorSets(VkResult *ret, VkDevice_T * _device, VkDescriptorPool_T * _descriptorPool, uint32_t _descriptorSetCount, const VkDescriptorSet * _pDescriptorSets) {
    *ret = (VkResult )vkFreeDescriptorSets(_device, _descriptorPool, _descriptorSetCount, _pDescriptorSets);
}
extern "C" void __c__vkUpdateDescriptorSets(VkDevice_T * _device, uint32_t _descriptorWriteCount, const VkWriteDescriptorSet * _pDescriptorWrites, uint32_t _descriptorCopyCount, const VkCopyDescriptorSet * _pDescriptorCopies) {
    vkUpdateDescriptorSets(_device, _descriptorWriteCount, _pDescriptorWrites, _descriptorCopyCount, _pDescriptorCopies);
}
extern "C" void __c__vkCreateFramebuffer(VkResult *ret, VkDevice_T * _device, const VkFramebufferCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkFramebuffer * _pFramebuffer) {
    *ret = (VkResult )vkCreateFramebuffer(_device, _pCreateInfo, _pAllocator, _pFramebuffer);
}
extern "C" void __c__vkDestroyFramebuffer(VkDevice_T * _device, VkFramebuffer_T * _framebuffer, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyFramebuffer(_device, _framebuffer, _pAllocator);
}
extern "C" void __c__vkCreateRenderPass(VkResult *ret, VkDevice_T * _device, const VkRenderPassCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkRenderPass * _pRenderPass) {
    *ret = (VkResult )vkCreateRenderPass(_device, _pCreateInfo, _pAllocator, _pRenderPass);
}
extern "C" void __c__vkDestroyRenderPass(VkDevice_T * _device, VkRenderPass_T * _renderPass, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyRenderPass(_device, _renderPass, _pAllocator);
}
extern "C" void __c__vkGetRenderAreaGranularity(VkDevice_T * _device, VkRenderPass_T * _renderPass, VkExtent2D * _pGranularity) {
    vkGetRenderAreaGranularity(_device, _renderPass, _pGranularity);
}
extern "C" void __c__vkCreateCommandPool(VkResult *ret, VkDevice_T * _device, const VkCommandPoolCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkCommandPool * _pCommandPool) {
    *ret = (VkResult )vkCreateCommandPool(_device, _pCreateInfo, _pAllocator, _pCommandPool);
}
extern "C" void __c__vkDestroyCommandPool(VkDevice_T * _device, VkCommandPool_T * _commandPool, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyCommandPool(_device, _commandPool, _pAllocator);
}
extern "C" void __c__vkResetCommandPool(VkResult *ret, VkDevice_T * _device, VkCommandPool_T * _commandPool, uint32_t _flags) {
    *ret = (VkResult )vkResetCommandPool(_device, _commandPool, _flags);
}
extern "C" void __c__vkAllocateCommandBuffers(VkResult *ret, VkDevice_T * _device, const VkCommandBufferAllocateInfo * _pAllocateInfo, VkCommandBuffer * _pCommandBuffers) {
    *ret = (VkResult )vkAllocateCommandBuffers(_device, _pAllocateInfo, _pCommandBuffers);
}
extern "C" void __c__vkFreeCommandBuffers(VkDevice_T * _device, VkCommandPool_T * _commandPool, uint32_t _commandBufferCount, const VkCommandBuffer * _pCommandBuffers) {
    vkFreeCommandBuffers(_device, _commandPool, _commandBufferCount, _pCommandBuffers);
}
extern "C" void __c__vkBeginCommandBuffer(VkResult *ret, VkCommandBuffer_T * _commandBuffer, const VkCommandBufferBeginInfo * _pBeginInfo) {
    *ret = (VkResult )vkBeginCommandBuffer(_commandBuffer, _pBeginInfo);
}
extern "C" void __c__vkEndCommandBuffer(VkResult *ret, VkCommandBuffer_T * _commandBuffer) {
    *ret = (VkResult )vkEndCommandBuffer(_commandBuffer);
}
extern "C" void __c__vkResetCommandBuffer(VkResult *ret, VkCommandBuffer_T * _commandBuffer, uint32_t _flags) {
    *ret = (VkResult )vkResetCommandBuffer(_commandBuffer, _flags);
}
extern "C" void __c__vkCmdBindPipeline(VkCommandBuffer_T * _commandBuffer, VkPipelineBindPoint _pipelineBindPoint, VkPipeline_T * _pipeline) {
    vkCmdBindPipeline(_commandBuffer, _pipelineBindPoint, _pipeline);
}
extern "C" void __c__vkCmdSetViewport(VkCommandBuffer_T * _commandBuffer, uint32_t _firstViewport, uint32_t _viewportCount, const VkViewport * _pViewports) {
    vkCmdSetViewport(_commandBuffer, _firstViewport, _viewportCount, _pViewports);
}
extern "C" void __c__vkCmdSetScissor(VkCommandBuffer_T * _commandBuffer, uint32_t _firstScissor, uint32_t _scissorCount, const VkRect2D * _pScissors) {
    vkCmdSetScissor(_commandBuffer, _firstScissor, _scissorCount, _pScissors);
}
extern "C" void __c__vkCmdSetLineWidth(VkCommandBuffer_T * _commandBuffer, float _lineWidth) {
    vkCmdSetLineWidth(_commandBuffer, _lineWidth);
}
extern "C" void __c__vkCmdSetDepthBias(VkCommandBuffer_T * _commandBuffer, float _depthBiasConstantFactor, float _depthBiasClamp, float _depthBiasSlopeFactor) {
    vkCmdSetDepthBias(_commandBuffer, _depthBiasConstantFactor, _depthBiasClamp, _depthBiasSlopeFactor);
}
extern "C" void __c__vkCmdSetBlendConstants(VkCommandBuffer_T * _commandBuffer, float * _blendConstants) {
    vkCmdSetBlendConstants(_commandBuffer, _blendConstants);
}
extern "C" void __c__vkCmdSetDepthBounds(VkCommandBuffer_T * _commandBuffer, float _minDepthBounds, float _maxDepthBounds) {
    vkCmdSetDepthBounds(_commandBuffer, _minDepthBounds, _maxDepthBounds);
}
extern "C" void __c__vkCmdSetStencilCompareMask(VkCommandBuffer_T * _commandBuffer, uint32_t _faceMask, uint32_t _compareMask) {
    vkCmdSetStencilCompareMask(_commandBuffer, _faceMask, _compareMask);
}
extern "C" void __c__vkCmdSetStencilWriteMask(VkCommandBuffer_T * _commandBuffer, uint32_t _faceMask, uint32_t _writeMask) {
    vkCmdSetStencilWriteMask(_commandBuffer, _faceMask, _writeMask);
}
extern "C" void __c__vkCmdSetStencilReference(VkCommandBuffer_T * _commandBuffer, uint32_t _faceMask, uint32_t _reference) {
    vkCmdSetStencilReference(_commandBuffer, _faceMask, _reference);
}
extern "C" void __c__vkCmdBindDescriptorSets(VkCommandBuffer_T * _commandBuffer, VkPipelineBindPoint _pipelineBindPoint, VkPipelineLayout_T * _layout, uint32_t _firstSet, uint32_t _descriptorSetCount, const VkDescriptorSet * _pDescriptorSets, uint32_t _dynamicOffsetCount, const uint32_t * _pDynamicOffsets) {
    vkCmdBindDescriptorSets(_commandBuffer, _pipelineBindPoint, _layout, _firstSet, _descriptorSetCount, _pDescriptorSets, _dynamicOffsetCount, _pDynamicOffsets);
}
extern "C" void __c__vkCmdBindIndexBuffer(VkCommandBuffer_T * _commandBuffer, VkBuffer_T * _buffer, uint64_t _offset, VkIndexType _indexType) {
    vkCmdBindIndexBuffer(_commandBuffer, _buffer, _offset, _indexType);
}
extern "C" void __c__vkCmdBindVertexBuffers(VkCommandBuffer_T * _commandBuffer, uint32_t _firstBinding, uint32_t _bindingCount, const VkBuffer * _pBuffers, const VkDeviceSize * _pOffsets) {
    vkCmdBindVertexBuffers(_commandBuffer, _firstBinding, _bindingCount, _pBuffers, _pOffsets);
}
extern "C" void __c__vkCmdDraw(VkCommandBuffer_T * _commandBuffer, uint32_t _vertexCount, uint32_t _instanceCount, uint32_t _firstVertex, uint32_t _firstInstance) {
    vkCmdDraw(_commandBuffer, _vertexCount, _instanceCount, _firstVertex, _firstInstance);
}
extern "C" void __c__vkCmdDrawIndexed(VkCommandBuffer_T * _commandBuffer, uint32_t _indexCount, uint32_t _instanceCount, uint32_t _firstIndex, int32_t _vertexOffset, uint32_t _firstInstance) {
    vkCmdDrawIndexed(_commandBuffer, _indexCount, _instanceCount, _firstIndex, _vertexOffset, _firstInstance);
}
extern "C" void __c__vkCmdDrawIndirect(VkCommandBuffer_T * _commandBuffer, VkBuffer_T * _buffer, uint64_t _offset, uint32_t _drawCount, uint32_t _stride) {
    vkCmdDrawIndirect(_commandBuffer, _buffer, _offset, _drawCount, _stride);
}
extern "C" void __c__vkCmdDrawIndexedIndirect(VkCommandBuffer_T * _commandBuffer, VkBuffer_T * _buffer, uint64_t _offset, uint32_t _drawCount, uint32_t _stride) {
    vkCmdDrawIndexedIndirect(_commandBuffer, _buffer, _offset, _drawCount, _stride);
}
extern "C" void __c__vkCmdDispatch(VkCommandBuffer_T * _commandBuffer, uint32_t _groupCountX, uint32_t _groupCountY, uint32_t _groupCountZ) {
    vkCmdDispatch(_commandBuffer, _groupCountX, _groupCountY, _groupCountZ);
}
extern "C" void __c__vkCmdDispatchIndirect(VkCommandBuffer_T * _commandBuffer, VkBuffer_T * _buffer, uint64_t _offset) {
    vkCmdDispatchIndirect(_commandBuffer, _buffer, _offset);
}
extern "C" void __c__vkCmdCopyBuffer(VkCommandBuffer_T * _commandBuffer, VkBuffer_T * _srcBuffer, VkBuffer_T * _dstBuffer, uint32_t _regionCount, const VkBufferCopy * _pRegions) {
    vkCmdCopyBuffer(_commandBuffer, _srcBuffer, _dstBuffer, _regionCount, _pRegions);
}
extern "C" void __c__vkCmdCopyImage(VkCommandBuffer_T * _commandBuffer, VkImage_T * _srcImage, VkImageLayout _srcImageLayout, VkImage_T * _dstImage, VkImageLayout _dstImageLayout, uint32_t _regionCount, const VkImageCopy * _pRegions) {
    vkCmdCopyImage(_commandBuffer, _srcImage, _srcImageLayout, _dstImage, _dstImageLayout, _regionCount, _pRegions);
}
extern "C" void __c__vkCmdBlitImage(VkCommandBuffer_T * _commandBuffer, VkImage_T * _srcImage, VkImageLayout _srcImageLayout, VkImage_T * _dstImage, VkImageLayout _dstImageLayout, uint32_t _regionCount, const VkImageBlit * _pRegions, VkFilter _filter) {
    vkCmdBlitImage(_commandBuffer, _srcImage, _srcImageLayout, _dstImage, _dstImageLayout, _regionCount, _pRegions, _filter);
}
extern "C" void __c__vkCmdCopyBufferToImage(VkCommandBuffer_T * _commandBuffer, VkBuffer_T * _srcBuffer, VkImage_T * _dstImage, VkImageLayout _dstImageLayout, uint32_t _regionCount, const VkBufferImageCopy * _pRegions) {
    vkCmdCopyBufferToImage(_commandBuffer, _srcBuffer, _dstImage, _dstImageLayout, _regionCount, _pRegions);
}
extern "C" void __c__vkCmdCopyImageToBuffer(VkCommandBuffer_T * _commandBuffer, VkImage_T * _srcImage, VkImageLayout _srcImageLayout, VkBuffer_T * _dstBuffer, uint32_t _regionCount, const VkBufferImageCopy * _pRegions) {
    vkCmdCopyImageToBuffer(_commandBuffer, _srcImage, _srcImageLayout, _dstBuffer, _regionCount, _pRegions);
}
extern "C" void __c__vkCmdUpdateBuffer(VkCommandBuffer_T * _commandBuffer, VkBuffer_T * _dstBuffer, uint64_t _dstOffset, uint64_t _dataSize, void * _pData) {
    vkCmdUpdateBuffer(_commandBuffer, _dstBuffer, _dstOffset, _dataSize, _pData);
}
extern "C" void __c__vkCmdFillBuffer(VkCommandBuffer_T * _commandBuffer, VkBuffer_T * _dstBuffer, uint64_t _dstOffset, uint64_t _size, uint32_t _data) {
    vkCmdFillBuffer(_commandBuffer, _dstBuffer, _dstOffset, _size, _data);
}
extern "C" void __c__vkCmdClearColorImage(VkCommandBuffer_T * _commandBuffer, VkImage_T * _image, VkImageLayout _imageLayout, const VkClearColorValue * _pColor, uint32_t _rangeCount, const VkImageSubresourceRange * _pRanges) {
    vkCmdClearColorImage(_commandBuffer, _image, _imageLayout, _pColor, _rangeCount, _pRanges);
}
extern "C" void __c__vkCmdClearDepthStencilImage(VkCommandBuffer_T * _commandBuffer, VkImage_T * _image, VkImageLayout _imageLayout, const VkClearDepthStencilValue * _pDepthStencil, uint32_t _rangeCount, const VkImageSubresourceRange * _pRanges) {
    vkCmdClearDepthStencilImage(_commandBuffer, _image, _imageLayout, _pDepthStencil, _rangeCount, _pRanges);
}
extern "C" void __c__vkCmdClearAttachments(VkCommandBuffer_T * _commandBuffer, uint32_t _attachmentCount, const VkClearAttachment * _pAttachments, uint32_t _rectCount, const VkClearRect * _pRects) {
    vkCmdClearAttachments(_commandBuffer, _attachmentCount, _pAttachments, _rectCount, _pRects);
}
extern "C" void __c__vkCmdResolveImage(VkCommandBuffer_T * _commandBuffer, VkImage_T * _srcImage, VkImageLayout _srcImageLayout, VkImage_T * _dstImage, VkImageLayout _dstImageLayout, uint32_t _regionCount, const VkImageResolve * _pRegions) {
    vkCmdResolveImage(_commandBuffer, _srcImage, _srcImageLayout, _dstImage, _dstImageLayout, _regionCount, _pRegions);
}
extern "C" void __c__vkCmdSetEvent(VkCommandBuffer_T * _commandBuffer, VkEvent_T * _event, uint32_t _stageMask) {
    vkCmdSetEvent(_commandBuffer, _event, _stageMask);
}
extern "C" void __c__vkCmdResetEvent(VkCommandBuffer_T * _commandBuffer, VkEvent_T * _event, uint32_t _stageMask) {
    vkCmdResetEvent(_commandBuffer, _event, _stageMask);
}
extern "C" void __c__vkCmdWaitEvents(VkCommandBuffer_T * _commandBuffer, uint32_t _eventCount, const VkEvent * _pEvents, uint32_t _srcStageMask, uint32_t _dstStageMask, uint32_t _memoryBarrierCount, const VkMemoryBarrier * _pMemoryBarriers, uint32_t _bufferMemoryBarrierCount, const VkBufferMemoryBarrier * _pBufferMemoryBarriers, uint32_t _imageMemoryBarrierCount, const VkImageMemoryBarrier * _pImageMemoryBarriers) {
    vkCmdWaitEvents(_commandBuffer, _eventCount, _pEvents, _srcStageMask, _dstStageMask, _memoryBarrierCount, _pMemoryBarriers, _bufferMemoryBarrierCount, _pBufferMemoryBarriers, _imageMemoryBarrierCount, _pImageMemoryBarriers);
}
extern "C" void __c__vkCmdPipelineBarrier(VkCommandBuffer_T * _commandBuffer, uint32_t _srcStageMask, uint32_t _dstStageMask, uint32_t _dependencyFlags, uint32_t _memoryBarrierCount, const VkMemoryBarrier * _pMemoryBarriers, uint32_t _bufferMemoryBarrierCount, const VkBufferMemoryBarrier * _pBufferMemoryBarriers, uint32_t _imageMemoryBarrierCount, const VkImageMemoryBarrier * _pImageMemoryBarriers) {
    vkCmdPipelineBarrier(_commandBuffer, _srcStageMask, _dstStageMask, _dependencyFlags, _memoryBarrierCount, _pMemoryBarriers, _bufferMemoryBarrierCount, _pBufferMemoryBarriers, _imageMemoryBarrierCount, _pImageMemoryBarriers);
}
extern "C" void __c__vkCmdBeginQuery(VkCommandBuffer_T * _commandBuffer, VkQueryPool_T * _queryPool, uint32_t _query, uint32_t _flags) {
    vkCmdBeginQuery(_commandBuffer, _queryPool, _query, _flags);
}
extern "C" void __c__vkCmdEndQuery(VkCommandBuffer_T * _commandBuffer, VkQueryPool_T * _queryPool, uint32_t _query) {
    vkCmdEndQuery(_commandBuffer, _queryPool, _query);
}
extern "C" void __c__vkCmdResetQueryPool(VkCommandBuffer_T * _commandBuffer, VkQueryPool_T * _queryPool, uint32_t _firstQuery, uint32_t _queryCount) {
    vkCmdResetQueryPool(_commandBuffer, _queryPool, _firstQuery, _queryCount);
}
extern "C" void __c__vkCmdWriteTimestamp(VkCommandBuffer_T * _commandBuffer, VkPipelineStageFlagBits _pipelineStage, VkQueryPool_T * _queryPool, uint32_t _query) {
    vkCmdWriteTimestamp(_commandBuffer, _pipelineStage, _queryPool, _query);
}
extern "C" void __c__vkCmdCopyQueryPoolResults(VkCommandBuffer_T * _commandBuffer, VkQueryPool_T * _queryPool, uint32_t _firstQuery, uint32_t _queryCount, VkBuffer_T * _dstBuffer, uint64_t _dstOffset, uint64_t _stride, uint32_t _flags) {
    vkCmdCopyQueryPoolResults(_commandBuffer, _queryPool, _firstQuery, _queryCount, _dstBuffer, _dstOffset, _stride, _flags);
}
extern "C" void __c__vkCmdPushConstants(VkCommandBuffer_T * _commandBuffer, VkPipelineLayout_T * _layout, uint32_t _stageFlags, uint32_t _offset, uint32_t _size, void * _pValues) {
    vkCmdPushConstants(_commandBuffer, _layout, _stageFlags, _offset, _size, _pValues);
}
extern "C" void __c__vkCmdBeginRenderPass(VkCommandBuffer_T * _commandBuffer, const VkRenderPassBeginInfo * _pRenderPassBegin, VkSubpassContents _contents) {
    vkCmdBeginRenderPass(_commandBuffer, _pRenderPassBegin, _contents);
}
extern "C" void __c__vkCmdNextSubpass(VkCommandBuffer_T * _commandBuffer, VkSubpassContents _contents) {
    vkCmdNextSubpass(_commandBuffer, _contents);
}
extern "C" void __c__vkCmdEndRenderPass(VkCommandBuffer_T * _commandBuffer) {
    vkCmdEndRenderPass(_commandBuffer);
}
extern "C" void __c__vkCmdExecuteCommands(VkCommandBuffer_T * _commandBuffer, uint32_t _commandBufferCount, const VkCommandBuffer * _pCommandBuffers) {
    vkCmdExecuteCommands(_commandBuffer, _commandBufferCount, _pCommandBuffers);
}
extern "C" void __c__vkEnumerateInstanceVersion(VkResult *ret, uint32_t * _pApiVersion) {
    *ret = (VkResult )vkEnumerateInstanceVersion(_pApiVersion);
}
extern "C" void __c__vkBindBufferMemory2(VkResult *ret, VkDevice_T * _device, uint32_t _bindInfoCount, const VkBindBufferMemoryInfo * _pBindInfos) {
    *ret = (VkResult )vkBindBufferMemory2(_device, _bindInfoCount, _pBindInfos);
}
extern "C" void __c__vkBindImageMemory2(VkResult *ret, VkDevice_T * _device, uint32_t _bindInfoCount, const VkBindImageMemoryInfo * _pBindInfos) {
    *ret = (VkResult )vkBindImageMemory2(_device, _bindInfoCount, _pBindInfos);
}
extern "C" void __c__vkGetDeviceGroupPeerMemoryFeatures(VkDevice_T * _device, uint32_t _heapIndex, uint32_t _localDeviceIndex, uint32_t _remoteDeviceIndex, VkPeerMemoryFeatureFlags * _pPeerMemoryFeatures) {
    vkGetDeviceGroupPeerMemoryFeatures(_device, _heapIndex, _localDeviceIndex, _remoteDeviceIndex, _pPeerMemoryFeatures);
}
extern "C" void __c__vkCmdSetDeviceMask(VkCommandBuffer_T * _commandBuffer, uint32_t _deviceMask) {
    vkCmdSetDeviceMask(_commandBuffer, _deviceMask);
}
extern "C" void __c__vkCmdDispatchBase(VkCommandBuffer_T * _commandBuffer, uint32_t _baseGroupX, uint32_t _baseGroupY, uint32_t _baseGroupZ, uint32_t _groupCountX, uint32_t _groupCountY, uint32_t _groupCountZ) {
    vkCmdDispatchBase(_commandBuffer, _baseGroupX, _baseGroupY, _baseGroupZ, _groupCountX, _groupCountY, _groupCountZ);
}
extern "C" void __c__vkEnumeratePhysicalDeviceGroups(VkResult *ret, VkInstance_T * _instance, uint32_t * _pPhysicalDeviceGroupCount, VkPhysicalDeviceGroupProperties * _pPhysicalDeviceGroupProperties) {
    *ret = (VkResult )vkEnumeratePhysicalDeviceGroups(_instance, _pPhysicalDeviceGroupCount, _pPhysicalDeviceGroupProperties);
}
extern "C" void __c__vkGetImageMemoryRequirements2(VkDevice_T * _device, const VkImageMemoryRequirementsInfo2 * _pInfo, VkMemoryRequirements2 * _pMemoryRequirements) {
    vkGetImageMemoryRequirements2(_device, _pInfo, _pMemoryRequirements);
}
extern "C" void __c__vkGetBufferMemoryRequirements2(VkDevice_T * _device, const VkBufferMemoryRequirementsInfo2 * _pInfo, VkMemoryRequirements2 * _pMemoryRequirements) {
    vkGetBufferMemoryRequirements2(_device, _pInfo, _pMemoryRequirements);
}
extern "C" void __c__vkGetImageSparseMemoryRequirements2(VkDevice_T * _device, const VkImageSparseMemoryRequirementsInfo2 * _pInfo, uint32_t * _pSparseMemoryRequirementCount, VkSparseImageMemoryRequirements2 * _pSparseMemoryRequirements) {
    vkGetImageSparseMemoryRequirements2(_device, _pInfo, _pSparseMemoryRequirementCount, _pSparseMemoryRequirements);
}
extern "C" void __c__vkGetPhysicalDeviceFeatures2(VkPhysicalDevice_T * _physicalDevice, VkPhysicalDeviceFeatures2 * _pFeatures) {
    vkGetPhysicalDeviceFeatures2(_physicalDevice, _pFeatures);
}
extern "C" void __c__vkGetPhysicalDeviceProperties2(VkPhysicalDevice_T * _physicalDevice, VkPhysicalDeviceProperties2 * _pProperties) {
    vkGetPhysicalDeviceProperties2(_physicalDevice, _pProperties);
}
extern "C" void __c__vkGetPhysicalDeviceFormatProperties2(VkPhysicalDevice_T * _physicalDevice, VkFormat _format, VkFormatProperties2 * _pFormatProperties) {
    vkGetPhysicalDeviceFormatProperties2(_physicalDevice, _format, _pFormatProperties);
}
extern "C" void __c__vkGetPhysicalDeviceImageFormatProperties2(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, const VkPhysicalDeviceImageFormatInfo2 * _pImageFormatInfo, VkImageFormatProperties2 * _pImageFormatProperties) {
    *ret = (VkResult )vkGetPhysicalDeviceImageFormatProperties2(_physicalDevice, _pImageFormatInfo, _pImageFormatProperties);
}
extern "C" void __c__vkGetPhysicalDeviceQueueFamilyProperties2(VkPhysicalDevice_T * _physicalDevice, uint32_t * _pQueueFamilyPropertyCount, VkQueueFamilyProperties2 * _pQueueFamilyProperties) {
    vkGetPhysicalDeviceQueueFamilyProperties2(_physicalDevice, _pQueueFamilyPropertyCount, _pQueueFamilyProperties);
}
extern "C" void __c__vkGetPhysicalDeviceMemoryProperties2(VkPhysicalDevice_T * _physicalDevice, VkPhysicalDeviceMemoryProperties2 * _pMemoryProperties) {
    vkGetPhysicalDeviceMemoryProperties2(_physicalDevice, _pMemoryProperties);
}
extern "C" void __c__vkGetPhysicalDeviceSparseImageFormatProperties2(VkPhysicalDevice_T * _physicalDevice, const VkPhysicalDeviceSparseImageFormatInfo2 * _pFormatInfo, uint32_t * _pPropertyCount, VkSparseImageFormatProperties2 * _pProperties) {
    vkGetPhysicalDeviceSparseImageFormatProperties2(_physicalDevice, _pFormatInfo, _pPropertyCount, _pProperties);
}
extern "C" void __c__vkTrimCommandPool(VkDevice_T * _device, VkCommandPool_T * _commandPool, uint32_t _flags) {
    vkTrimCommandPool(_device, _commandPool, _flags);
}
extern "C" void __c__vkGetDeviceQueue2(VkDevice_T * _device, const VkDeviceQueueInfo2 * _pQueueInfo, VkQueue * _pQueue) {
    vkGetDeviceQueue2(_device, _pQueueInfo, _pQueue);
}
extern "C" void __c__vkCreateSamplerYcbcrConversion(VkResult *ret, VkDevice_T * _device, const VkSamplerYcbcrConversionCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkSamplerYcbcrConversion * _pYcbcrConversion) {
    *ret = (VkResult )vkCreateSamplerYcbcrConversion(_device, _pCreateInfo, _pAllocator, _pYcbcrConversion);
}
extern "C" void __c__vkDestroySamplerYcbcrConversion(VkDevice_T * _device, VkSamplerYcbcrConversion_T * _ycbcrConversion, const VkAllocationCallbacks * _pAllocator) {
    vkDestroySamplerYcbcrConversion(_device, _ycbcrConversion, _pAllocator);
}
extern "C" void __c__vkCreateDescriptorUpdateTemplate(VkResult *ret, VkDevice_T * _device, const VkDescriptorUpdateTemplateCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkDescriptorUpdateTemplate * _pDescriptorUpdateTemplate) {
    *ret = (VkResult )vkCreateDescriptorUpdateTemplate(_device, _pCreateInfo, _pAllocator, _pDescriptorUpdateTemplate);
}
extern "C" void __c__vkDestroyDescriptorUpdateTemplate(VkDevice_T * _device, VkDescriptorUpdateTemplate_T * _descriptorUpdateTemplate, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyDescriptorUpdateTemplate(_device, _descriptorUpdateTemplate, _pAllocator);
}
extern "C" void __c__vkUpdateDescriptorSetWithTemplate(VkDevice_T * _device, VkDescriptorSet_T * _descriptorSet, VkDescriptorUpdateTemplate_T * _descriptorUpdateTemplate, void * _pData) {
    vkUpdateDescriptorSetWithTemplate(_device, _descriptorSet, _descriptorUpdateTemplate, _pData);
}
extern "C" void __c__vkGetPhysicalDeviceExternalBufferProperties(VkPhysicalDevice_T * _physicalDevice, const VkPhysicalDeviceExternalBufferInfo * _pExternalBufferInfo, VkExternalBufferProperties * _pExternalBufferProperties) {
    vkGetPhysicalDeviceExternalBufferProperties(_physicalDevice, _pExternalBufferInfo, _pExternalBufferProperties);
}
extern "C" void __c__vkGetPhysicalDeviceExternalFenceProperties(VkPhysicalDevice_T * _physicalDevice, const VkPhysicalDeviceExternalFenceInfo * _pExternalFenceInfo, VkExternalFenceProperties * _pExternalFenceProperties) {
    vkGetPhysicalDeviceExternalFenceProperties(_physicalDevice, _pExternalFenceInfo, _pExternalFenceProperties);
}
extern "C" void __c__vkGetPhysicalDeviceExternalSemaphoreProperties(VkPhysicalDevice_T * _physicalDevice, const VkPhysicalDeviceExternalSemaphoreInfo * _pExternalSemaphoreInfo, VkExternalSemaphoreProperties * _pExternalSemaphoreProperties) {
    vkGetPhysicalDeviceExternalSemaphoreProperties(_physicalDevice, _pExternalSemaphoreInfo, _pExternalSemaphoreProperties);
}
extern "C" void __c__vkGetDescriptorSetLayoutSupport(VkDevice_T * _device, const VkDescriptorSetLayoutCreateInfo * _pCreateInfo, VkDescriptorSetLayoutSupport * _pSupport) {
    vkGetDescriptorSetLayoutSupport(_device, _pCreateInfo, _pSupport);
}
extern "C" void __c__vkCmdDrawIndirectCount(VkCommandBuffer_T * _commandBuffer, VkBuffer_T * _buffer, uint64_t _offset, VkBuffer_T * _countBuffer, uint64_t _countBufferOffset, uint32_t _maxDrawCount, uint32_t _stride) {
    vkCmdDrawIndirectCount(_commandBuffer, _buffer, _offset, _countBuffer, _countBufferOffset, _maxDrawCount, _stride);
}
extern "C" void __c__vkCmdDrawIndexedIndirectCount(VkCommandBuffer_T * _commandBuffer, VkBuffer_T * _buffer, uint64_t _offset, VkBuffer_T * _countBuffer, uint64_t _countBufferOffset, uint32_t _maxDrawCount, uint32_t _stride) {
    vkCmdDrawIndexedIndirectCount(_commandBuffer, _buffer, _offset, _countBuffer, _countBufferOffset, _maxDrawCount, _stride);
}
extern "C" void __c__vkCreateRenderPass2(VkResult *ret, VkDevice_T * _device, const VkRenderPassCreateInfo2 * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkRenderPass * _pRenderPass) {
    *ret = (VkResult )vkCreateRenderPass2(_device, _pCreateInfo, _pAllocator, _pRenderPass);
}
extern "C" void __c__vkCmdBeginRenderPass2(VkCommandBuffer_T * _commandBuffer, const VkRenderPassBeginInfo * _pRenderPassBegin, const VkSubpassBeginInfo * _pSubpassBeginInfo) {
    vkCmdBeginRenderPass2(_commandBuffer, _pRenderPassBegin, _pSubpassBeginInfo);
}
extern "C" void __c__vkCmdNextSubpass2(VkCommandBuffer_T * _commandBuffer, const VkSubpassBeginInfo * _pSubpassBeginInfo, const VkSubpassEndInfo * _pSubpassEndInfo) {
    vkCmdNextSubpass2(_commandBuffer, _pSubpassBeginInfo, _pSubpassEndInfo);
}
extern "C" void __c__vkCmdEndRenderPass2(VkCommandBuffer_T * _commandBuffer, const VkSubpassEndInfo * _pSubpassEndInfo) {
    vkCmdEndRenderPass2(_commandBuffer, _pSubpassEndInfo);
}
extern "C" void __c__vkResetQueryPool(VkDevice_T * _device, VkQueryPool_T * _queryPool, uint32_t _firstQuery, uint32_t _queryCount) {
    vkResetQueryPool(_device, _queryPool, _firstQuery, _queryCount);
}
extern "C" void __c__vkGetSemaphoreCounterValue(VkResult *ret, VkDevice_T * _device, VkSemaphore_T * _semaphore, uint64_t * _pValue) {
    *ret = (VkResult )vkGetSemaphoreCounterValue(_device, _semaphore, _pValue);
}
extern "C" void __c__vkWaitSemaphores(VkResult *ret, VkDevice_T * _device, const VkSemaphoreWaitInfo * _pWaitInfo, uint64_t _timeout) {
    *ret = (VkResult )vkWaitSemaphores(_device, _pWaitInfo, _timeout);
}
extern "C" void __c__vkSignalSemaphore(VkResult *ret, VkDevice_T * _device, const VkSemaphoreSignalInfo * _pSignalInfo) {
    *ret = (VkResult )vkSignalSemaphore(_device, _pSignalInfo);
}
extern "C" void __c__vkGetBufferDeviceAddress(VkDeviceAddress *ret, VkDevice_T * _device, const VkBufferDeviceAddressInfo * _pInfo) {
    *ret = (uint64_t )vkGetBufferDeviceAddress(_device, _pInfo);
}
extern "C" void __c__vkGetBufferOpaqueCaptureAddress(uint64_t *ret, VkDevice_T * _device, const VkBufferDeviceAddressInfo * _pInfo) {
    *ret = (uint64_t )vkGetBufferOpaqueCaptureAddress(_device, _pInfo);
}
extern "C" void __c__vkGetDeviceMemoryOpaqueCaptureAddress(uint64_t *ret, VkDevice_T * _device, const VkDeviceMemoryOpaqueCaptureAddressInfo * _pInfo) {
    *ret = (uint64_t )vkGetDeviceMemoryOpaqueCaptureAddress(_device, _pInfo);
}
extern "C" void __c__vkDestroySurfaceKHR(VkInstance_T * _instance, VkSurfaceKHR_T * _surface, const VkAllocationCallbacks * _pAllocator) {
    vkDestroySurfaceKHR(_instance, _surface, _pAllocator);
}
extern "C" void __c__vkGetPhysicalDeviceSurfaceSupportKHR(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, uint32_t _queueFamilyIndex, VkSurfaceKHR_T * _surface, VkBool32 * _pSupported) {
    *ret = (VkResult )vkGetPhysicalDeviceSurfaceSupportKHR(_physicalDevice, _queueFamilyIndex, _surface, _pSupported);
}
extern "C" void __c__vkGetPhysicalDeviceSurfaceCapabilitiesKHR(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, VkSurfaceKHR_T * _surface, VkSurfaceCapabilitiesKHR * _pSurfaceCapabilities) {
    *ret = (VkResult )vkGetPhysicalDeviceSurfaceCapabilitiesKHR(_physicalDevice, _surface, _pSurfaceCapabilities);
}
extern "C" void __c__vkGetPhysicalDeviceSurfaceFormatsKHR(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, VkSurfaceKHR_T * _surface, uint32_t * _pSurfaceFormatCount, VkSurfaceFormatKHR * _pSurfaceFormats) {
    *ret = (VkResult )vkGetPhysicalDeviceSurfaceFormatsKHR(_physicalDevice, _surface, _pSurfaceFormatCount, _pSurfaceFormats);
}
extern "C" void __c__vkGetPhysicalDeviceSurfacePresentModesKHR(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, VkSurfaceKHR_T * _surface, uint32_t * _pPresentModeCount, VkPresentModeKHR * _pPresentModes) {
    *ret = (VkResult )vkGetPhysicalDeviceSurfacePresentModesKHR(_physicalDevice, _surface, _pPresentModeCount, _pPresentModes);
}
extern "C" void __c__vkCreateSwapchainKHR(VkResult *ret, VkDevice_T * _device, const VkSwapchainCreateInfoKHR * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkSwapchainKHR * _pSwapchain) {
    *ret = (VkResult )vkCreateSwapchainKHR(_device, _pCreateInfo, _pAllocator, _pSwapchain);
}
extern "C" void __c__vkDestroySwapchainKHR(VkDevice_T * _device, VkSwapchainKHR_T * _swapchain, const VkAllocationCallbacks * _pAllocator) {
    vkDestroySwapchainKHR(_device, _swapchain, _pAllocator);
}
extern "C" void __c__vkGetSwapchainImagesKHR(VkResult *ret, VkDevice_T * _device, VkSwapchainKHR_T * _swapchain, uint32_t * _pSwapchainImageCount, VkImage * _pSwapchainImages) {
    *ret = (VkResult )vkGetSwapchainImagesKHR(_device, _swapchain, _pSwapchainImageCount, _pSwapchainImages);
}
extern "C" void __c__vkAcquireNextImageKHR(VkResult *ret, VkDevice_T * _device, VkSwapchainKHR_T * _swapchain, uint64_t _timeout, VkSemaphore_T * _semaphore, VkFence_T * _fence, uint32_t * _pImageIndex) {
    *ret = (VkResult )vkAcquireNextImageKHR(_device, _swapchain, _timeout, _semaphore, _fence, _pImageIndex);
}
extern "C" void __c__vkQueuePresentKHR(VkResult *ret, VkQueue_T * _queue, const VkPresentInfoKHR * _pPresentInfo) {
    *ret = (VkResult )vkQueuePresentKHR(_queue, _pPresentInfo);
}
extern "C" void __c__vkGetDeviceGroupPresentCapabilitiesKHR(VkResult *ret, VkDevice_T * _device, VkDeviceGroupPresentCapabilitiesKHR * _pDeviceGroupPresentCapabilities) {
    *ret = (VkResult )vkGetDeviceGroupPresentCapabilitiesKHR(_device, _pDeviceGroupPresentCapabilities);
}
extern "C" void __c__vkGetDeviceGroupSurfacePresentModesKHR(VkResult *ret, VkDevice_T * _device, VkSurfaceKHR_T * _surface, VkDeviceGroupPresentModeFlagsKHR * _pModes) {
    *ret = (VkResult )vkGetDeviceGroupSurfacePresentModesKHR(_device, _surface, _pModes);
}
extern "C" void __c__vkGetPhysicalDevicePresentRectanglesKHR(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, VkSurfaceKHR_T * _surface, uint32_t * _pRectCount, VkRect2D * _pRects) {
    *ret = (VkResult )vkGetPhysicalDevicePresentRectanglesKHR(_physicalDevice, _surface, _pRectCount, _pRects);
}
extern "C" void __c__vkAcquireNextImage2KHR(VkResult *ret, VkDevice_T * _device, const VkAcquireNextImageInfoKHR * _pAcquireInfo, uint32_t * _pImageIndex) {
    *ret = (VkResult )vkAcquireNextImage2KHR(_device, _pAcquireInfo, _pImageIndex);
}
extern "C" void __c__vkGetPhysicalDeviceDisplayPropertiesKHR(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, uint32_t * _pPropertyCount, VkDisplayPropertiesKHR * _pProperties) {
    *ret = (VkResult )vkGetPhysicalDeviceDisplayPropertiesKHR(_physicalDevice, _pPropertyCount, _pProperties);
}
extern "C" void __c__vkGetPhysicalDeviceDisplayPlanePropertiesKHR(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, uint32_t * _pPropertyCount, VkDisplayPlanePropertiesKHR * _pProperties) {
    *ret = (VkResult )vkGetPhysicalDeviceDisplayPlanePropertiesKHR(_physicalDevice, _pPropertyCount, _pProperties);
}
extern "C" void __c__vkGetDisplayPlaneSupportedDisplaysKHR(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, uint32_t _planeIndex, uint32_t * _pDisplayCount, VkDisplayKHR * _pDisplays) {
    *ret = (VkResult )vkGetDisplayPlaneSupportedDisplaysKHR(_physicalDevice, _planeIndex, _pDisplayCount, _pDisplays);
}
extern "C" void __c__vkGetDisplayModePropertiesKHR(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, VkDisplayKHR_T * _display, uint32_t * _pPropertyCount, VkDisplayModePropertiesKHR * _pProperties) {
    *ret = (VkResult )vkGetDisplayModePropertiesKHR(_physicalDevice, _display, _pPropertyCount, _pProperties);
}
extern "C" void __c__vkCreateDisplayModeKHR(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, VkDisplayKHR_T * _display, const VkDisplayModeCreateInfoKHR * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkDisplayModeKHR * _pMode) {
    *ret = (VkResult )vkCreateDisplayModeKHR(_physicalDevice, _display, _pCreateInfo, _pAllocator, _pMode);
}
extern "C" void __c__vkGetDisplayPlaneCapabilitiesKHR(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, VkDisplayModeKHR_T * _mode, uint32_t _planeIndex, VkDisplayPlaneCapabilitiesKHR * _pCapabilities) {
    *ret = (VkResult )vkGetDisplayPlaneCapabilitiesKHR(_physicalDevice, _mode, _planeIndex, _pCapabilities);
}
extern "C" void __c__vkCreateDisplayPlaneSurfaceKHR(VkResult *ret, VkInstance_T * _instance, const VkDisplaySurfaceCreateInfoKHR * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkSurfaceKHR * _pSurface) {
    *ret = (VkResult )vkCreateDisplayPlaneSurfaceKHR(_instance, _pCreateInfo, _pAllocator, _pSurface);
}
extern "C" void __c__vkCreateSharedSwapchainsKHR(VkResult *ret, VkDevice_T * _device, uint32_t _swapchainCount, const VkSwapchainCreateInfoKHR * _pCreateInfos, const VkAllocationCallbacks * _pAllocator, VkSwapchainKHR * _pSwapchains) {
    *ret = (VkResult )vkCreateSharedSwapchainsKHR(_device, _swapchainCount, _pCreateInfos, _pAllocator, _pSwapchains);
}
extern "C" void __c__vkGetPhysicalDeviceFeatures2KHR(VkPhysicalDevice_T * _physicalDevice, VkPhysicalDeviceFeatures2 * _pFeatures) {
    vkGetPhysicalDeviceFeatures2KHR(_physicalDevice, _pFeatures);
}
extern "C" void __c__vkGetPhysicalDeviceProperties2KHR(VkPhysicalDevice_T * _physicalDevice, VkPhysicalDeviceProperties2 * _pProperties) {
    vkGetPhysicalDeviceProperties2KHR(_physicalDevice, _pProperties);
}
extern "C" void __c__vkGetPhysicalDeviceFormatProperties2KHR(VkPhysicalDevice_T * _physicalDevice, VkFormat _format, VkFormatProperties2 * _pFormatProperties) {
    vkGetPhysicalDeviceFormatProperties2KHR(_physicalDevice, _format, _pFormatProperties);
}
extern "C" void __c__vkGetPhysicalDeviceImageFormatProperties2KHR(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, const VkPhysicalDeviceImageFormatInfo2 * _pImageFormatInfo, VkImageFormatProperties2 * _pImageFormatProperties) {
    *ret = (VkResult )vkGetPhysicalDeviceImageFormatProperties2KHR(_physicalDevice, _pImageFormatInfo, _pImageFormatProperties);
}
extern "C" void __c__vkGetPhysicalDeviceQueueFamilyProperties2KHR(VkPhysicalDevice_T * _physicalDevice, uint32_t * _pQueueFamilyPropertyCount, VkQueueFamilyProperties2 * _pQueueFamilyProperties) {
    vkGetPhysicalDeviceQueueFamilyProperties2KHR(_physicalDevice, _pQueueFamilyPropertyCount, _pQueueFamilyProperties);
}
extern "C" void __c__vkGetPhysicalDeviceMemoryProperties2KHR(VkPhysicalDevice_T * _physicalDevice, VkPhysicalDeviceMemoryProperties2 * _pMemoryProperties) {
    vkGetPhysicalDeviceMemoryProperties2KHR(_physicalDevice, _pMemoryProperties);
}
extern "C" void __c__vkGetPhysicalDeviceSparseImageFormatProperties2KHR(VkPhysicalDevice_T * _physicalDevice, const VkPhysicalDeviceSparseImageFormatInfo2 * _pFormatInfo, uint32_t * _pPropertyCount, VkSparseImageFormatProperties2 * _pProperties) {
    vkGetPhysicalDeviceSparseImageFormatProperties2KHR(_physicalDevice, _pFormatInfo, _pPropertyCount, _pProperties);
}
extern "C" void __c__vkGetDeviceGroupPeerMemoryFeaturesKHR(VkDevice_T * _device, uint32_t _heapIndex, uint32_t _localDeviceIndex, uint32_t _remoteDeviceIndex, VkPeerMemoryFeatureFlags * _pPeerMemoryFeatures) {
    vkGetDeviceGroupPeerMemoryFeaturesKHR(_device, _heapIndex, _localDeviceIndex, _remoteDeviceIndex, _pPeerMemoryFeatures);
}
extern "C" void __c__vkCmdSetDeviceMaskKHR(VkCommandBuffer_T * _commandBuffer, uint32_t _deviceMask) {
    vkCmdSetDeviceMaskKHR(_commandBuffer, _deviceMask);
}
extern "C" void __c__vkCmdDispatchBaseKHR(VkCommandBuffer_T * _commandBuffer, uint32_t _baseGroupX, uint32_t _baseGroupY, uint32_t _baseGroupZ, uint32_t _groupCountX, uint32_t _groupCountY, uint32_t _groupCountZ) {
    vkCmdDispatchBaseKHR(_commandBuffer, _baseGroupX, _baseGroupY, _baseGroupZ, _groupCountX, _groupCountY, _groupCountZ);
}
extern "C" void __c__vkTrimCommandPoolKHR(VkDevice_T * _device, VkCommandPool_T * _commandPool, uint32_t _flags) {
    vkTrimCommandPoolKHR(_device, _commandPool, _flags);
}
extern "C" void __c__vkEnumeratePhysicalDeviceGroupsKHR(VkResult *ret, VkInstance_T * _instance, uint32_t * _pPhysicalDeviceGroupCount, VkPhysicalDeviceGroupProperties * _pPhysicalDeviceGroupProperties) {
    *ret = (VkResult )vkEnumeratePhysicalDeviceGroupsKHR(_instance, _pPhysicalDeviceGroupCount, _pPhysicalDeviceGroupProperties);
}
extern "C" void __c__vkGetPhysicalDeviceExternalBufferPropertiesKHR(VkPhysicalDevice_T * _physicalDevice, const VkPhysicalDeviceExternalBufferInfo * _pExternalBufferInfo, VkExternalBufferProperties * _pExternalBufferProperties) {
    vkGetPhysicalDeviceExternalBufferPropertiesKHR(_physicalDevice, _pExternalBufferInfo, _pExternalBufferProperties);
}
extern "C" void __c__vkGetMemoryFdKHR(VkResult *ret, VkDevice_T * _device, const VkMemoryGetFdInfoKHR * _pGetFdInfo, int32_t * _pFd) {
    *ret = (VkResult )vkGetMemoryFdKHR(_device, _pGetFdInfo, _pFd);
}
extern "C" void __c__vkGetMemoryFdPropertiesKHR(VkResult *ret, VkDevice_T * _device, VkExternalMemoryHandleTypeFlagBits _handleType, int32_t _fd, VkMemoryFdPropertiesKHR * _pMemoryFdProperties) {
    *ret = (VkResult )vkGetMemoryFdPropertiesKHR(_device, _handleType, _fd, _pMemoryFdProperties);
}
extern "C" void __c__vkGetPhysicalDeviceExternalSemaphorePropertiesKHR(VkPhysicalDevice_T * _physicalDevice, const VkPhysicalDeviceExternalSemaphoreInfo * _pExternalSemaphoreInfo, VkExternalSemaphoreProperties * _pExternalSemaphoreProperties) {
    vkGetPhysicalDeviceExternalSemaphorePropertiesKHR(_physicalDevice, _pExternalSemaphoreInfo, _pExternalSemaphoreProperties);
}
extern "C" void __c__vkImportSemaphoreFdKHR(VkResult *ret, VkDevice_T * _device, const VkImportSemaphoreFdInfoKHR * _pImportSemaphoreFdInfo) {
    *ret = (VkResult )vkImportSemaphoreFdKHR(_device, _pImportSemaphoreFdInfo);
}
extern "C" void __c__vkGetSemaphoreFdKHR(VkResult *ret, VkDevice_T * _device, const VkSemaphoreGetFdInfoKHR * _pGetFdInfo, int32_t * _pFd) {
    *ret = (VkResult )vkGetSemaphoreFdKHR(_device, _pGetFdInfo, _pFd);
}
extern "C" void __c__vkCmdPushDescriptorSetKHR(VkCommandBuffer_T * _commandBuffer, VkPipelineBindPoint _pipelineBindPoint, VkPipelineLayout_T * _layout, uint32_t _set, uint32_t _descriptorWriteCount, const VkWriteDescriptorSet * _pDescriptorWrites) {
    vkCmdPushDescriptorSetKHR(_commandBuffer, _pipelineBindPoint, _layout, _set, _descriptorWriteCount, _pDescriptorWrites);
}
extern "C" void __c__vkCmdPushDescriptorSetWithTemplateKHR(VkCommandBuffer_T * _commandBuffer, VkDescriptorUpdateTemplate_T * _descriptorUpdateTemplate, VkPipelineLayout_T * _layout, uint32_t _set, void * _pData) {
    vkCmdPushDescriptorSetWithTemplateKHR(_commandBuffer, _descriptorUpdateTemplate, _layout, _set, _pData);
}
extern "C" void __c__vkCreateDescriptorUpdateTemplateKHR(VkResult *ret, VkDevice_T * _device, const VkDescriptorUpdateTemplateCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkDescriptorUpdateTemplate * _pDescriptorUpdateTemplate) {
    *ret = (VkResult )vkCreateDescriptorUpdateTemplateKHR(_device, _pCreateInfo, _pAllocator, _pDescriptorUpdateTemplate);
}
extern "C" void __c__vkDestroyDescriptorUpdateTemplateKHR(VkDevice_T * _device, VkDescriptorUpdateTemplate_T * _descriptorUpdateTemplate, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyDescriptorUpdateTemplateKHR(_device, _descriptorUpdateTemplate, _pAllocator);
}
extern "C" void __c__vkUpdateDescriptorSetWithTemplateKHR(VkDevice_T * _device, VkDescriptorSet_T * _descriptorSet, VkDescriptorUpdateTemplate_T * _descriptorUpdateTemplate, void * _pData) {
    vkUpdateDescriptorSetWithTemplateKHR(_device, _descriptorSet, _descriptorUpdateTemplate, _pData);
}
extern "C" void __c__vkCreateRenderPass2KHR(VkResult *ret, VkDevice_T * _device, const VkRenderPassCreateInfo2 * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkRenderPass * _pRenderPass) {
    *ret = (VkResult )vkCreateRenderPass2KHR(_device, _pCreateInfo, _pAllocator, _pRenderPass);
}
extern "C" void __c__vkCmdBeginRenderPass2KHR(VkCommandBuffer_T * _commandBuffer, const VkRenderPassBeginInfo * _pRenderPassBegin, const VkSubpassBeginInfo * _pSubpassBeginInfo) {
    vkCmdBeginRenderPass2KHR(_commandBuffer, _pRenderPassBegin, _pSubpassBeginInfo);
}
extern "C" void __c__vkCmdNextSubpass2KHR(VkCommandBuffer_T * _commandBuffer, const VkSubpassBeginInfo * _pSubpassBeginInfo, const VkSubpassEndInfo * _pSubpassEndInfo) {
    vkCmdNextSubpass2KHR(_commandBuffer, _pSubpassBeginInfo, _pSubpassEndInfo);
}
extern "C" void __c__vkCmdEndRenderPass2KHR(VkCommandBuffer_T * _commandBuffer, const VkSubpassEndInfo * _pSubpassEndInfo) {
    vkCmdEndRenderPass2KHR(_commandBuffer, _pSubpassEndInfo);
}
extern "C" void __c__vkGetSwapchainStatusKHR(VkResult *ret, VkDevice_T * _device, VkSwapchainKHR_T * _swapchain) {
    *ret = (VkResult )vkGetSwapchainStatusKHR(_device, _swapchain);
}
extern "C" void __c__vkGetPhysicalDeviceExternalFencePropertiesKHR(VkPhysicalDevice_T * _physicalDevice, const VkPhysicalDeviceExternalFenceInfo * _pExternalFenceInfo, VkExternalFenceProperties * _pExternalFenceProperties) {
    vkGetPhysicalDeviceExternalFencePropertiesKHR(_physicalDevice, _pExternalFenceInfo, _pExternalFenceProperties);
}
extern "C" void __c__vkImportFenceFdKHR(VkResult *ret, VkDevice_T * _device, const VkImportFenceFdInfoKHR * _pImportFenceFdInfo) {
    *ret = (VkResult )vkImportFenceFdKHR(_device, _pImportFenceFdInfo);
}
extern "C" void __c__vkGetFenceFdKHR(VkResult *ret, VkDevice_T * _device, const VkFenceGetFdInfoKHR * _pGetFdInfo, int32_t * _pFd) {
    *ret = (VkResult )vkGetFenceFdKHR(_device, _pGetFdInfo, _pFd);
}
extern "C" void __c__vkEnumeratePhysicalDeviceQueueFamilyPerformanceQueryCountersKHR(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, uint32_t _queueFamilyIndex, uint32_t * _pCounterCount, VkPerformanceCounterKHR * _pCounters, VkPerformanceCounterDescriptionKHR * _pCounterDescriptions) {
    *ret = (VkResult )vkEnumeratePhysicalDeviceQueueFamilyPerformanceQueryCountersKHR(_physicalDevice, _queueFamilyIndex, _pCounterCount, _pCounters, _pCounterDescriptions);
}
extern "C" void __c__vkGetPhysicalDeviceQueueFamilyPerformanceQueryPassesKHR(VkPhysicalDevice_T * _physicalDevice, const VkQueryPoolPerformanceCreateInfoKHR * _pPerformanceQueryCreateInfo, uint32_t * _pNumPasses) {
    vkGetPhysicalDeviceQueueFamilyPerformanceQueryPassesKHR(_physicalDevice, _pPerformanceQueryCreateInfo, _pNumPasses);
}
extern "C" void __c__vkAcquireProfilingLockKHR(VkResult *ret, VkDevice_T * _device, const VkAcquireProfilingLockInfoKHR * _pInfo) {
    *ret = (VkResult )vkAcquireProfilingLockKHR(_device, _pInfo);
}
extern "C" void __c__vkReleaseProfilingLockKHR(VkDevice_T * _device) {
    vkReleaseProfilingLockKHR(_device);
}
extern "C" void __c__vkGetPhysicalDeviceSurfaceCapabilities2KHR(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, const VkPhysicalDeviceSurfaceInfo2KHR * _pSurfaceInfo, VkSurfaceCapabilities2KHR * _pSurfaceCapabilities) {
    *ret = (VkResult )vkGetPhysicalDeviceSurfaceCapabilities2KHR(_physicalDevice, _pSurfaceInfo, _pSurfaceCapabilities);
}
extern "C" void __c__vkGetPhysicalDeviceSurfaceFormats2KHR(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, const VkPhysicalDeviceSurfaceInfo2KHR * _pSurfaceInfo, uint32_t * _pSurfaceFormatCount, VkSurfaceFormat2KHR * _pSurfaceFormats) {
    *ret = (VkResult )vkGetPhysicalDeviceSurfaceFormats2KHR(_physicalDevice, _pSurfaceInfo, _pSurfaceFormatCount, _pSurfaceFormats);
}
extern "C" void __c__vkGetPhysicalDeviceDisplayProperties2KHR(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, uint32_t * _pPropertyCount, VkDisplayProperties2KHR * _pProperties) {
    *ret = (VkResult )vkGetPhysicalDeviceDisplayProperties2KHR(_physicalDevice, _pPropertyCount, _pProperties);
}
extern "C" void __c__vkGetPhysicalDeviceDisplayPlaneProperties2KHR(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, uint32_t * _pPropertyCount, VkDisplayPlaneProperties2KHR * _pProperties) {
    *ret = (VkResult )vkGetPhysicalDeviceDisplayPlaneProperties2KHR(_physicalDevice, _pPropertyCount, _pProperties);
}
extern "C" void __c__vkGetDisplayModeProperties2KHR(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, VkDisplayKHR_T * _display, uint32_t * _pPropertyCount, VkDisplayModeProperties2KHR * _pProperties) {
    *ret = (VkResult )vkGetDisplayModeProperties2KHR(_physicalDevice, _display, _pPropertyCount, _pProperties);
}
extern "C" void __c__vkGetDisplayPlaneCapabilities2KHR(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, const VkDisplayPlaneInfo2KHR * _pDisplayPlaneInfo, VkDisplayPlaneCapabilities2KHR * _pCapabilities) {
    *ret = (VkResult )vkGetDisplayPlaneCapabilities2KHR(_physicalDevice, _pDisplayPlaneInfo, _pCapabilities);
}
extern "C" void __c__vkGetImageMemoryRequirements2KHR(VkDevice_T * _device, const VkImageMemoryRequirementsInfo2 * _pInfo, VkMemoryRequirements2 * _pMemoryRequirements) {
    vkGetImageMemoryRequirements2KHR(_device, _pInfo, _pMemoryRequirements);
}
extern "C" void __c__vkGetBufferMemoryRequirements2KHR(VkDevice_T * _device, const VkBufferMemoryRequirementsInfo2 * _pInfo, VkMemoryRequirements2 * _pMemoryRequirements) {
    vkGetBufferMemoryRequirements2KHR(_device, _pInfo, _pMemoryRequirements);
}
extern "C" void __c__vkGetImageSparseMemoryRequirements2KHR(VkDevice_T * _device, const VkImageSparseMemoryRequirementsInfo2 * _pInfo, uint32_t * _pSparseMemoryRequirementCount, VkSparseImageMemoryRequirements2 * _pSparseMemoryRequirements) {
    vkGetImageSparseMemoryRequirements2KHR(_device, _pInfo, _pSparseMemoryRequirementCount, _pSparseMemoryRequirements);
}
extern "C" void __c__vkCreateSamplerYcbcrConversionKHR(VkResult *ret, VkDevice_T * _device, const VkSamplerYcbcrConversionCreateInfo * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkSamplerYcbcrConversion * _pYcbcrConversion) {
    *ret = (VkResult )vkCreateSamplerYcbcrConversionKHR(_device, _pCreateInfo, _pAllocator, _pYcbcrConversion);
}
extern "C" void __c__vkDestroySamplerYcbcrConversionKHR(VkDevice_T * _device, VkSamplerYcbcrConversion_T * _ycbcrConversion, const VkAllocationCallbacks * _pAllocator) {
    vkDestroySamplerYcbcrConversionKHR(_device, _ycbcrConversion, _pAllocator);
}
extern "C" void __c__vkBindBufferMemory2KHR(VkResult *ret, VkDevice_T * _device, uint32_t _bindInfoCount, const VkBindBufferMemoryInfo * _pBindInfos) {
    *ret = (VkResult )vkBindBufferMemory2KHR(_device, _bindInfoCount, _pBindInfos);
}
extern "C" void __c__vkBindImageMemory2KHR(VkResult *ret, VkDevice_T * _device, uint32_t _bindInfoCount, const VkBindImageMemoryInfo * _pBindInfos) {
    *ret = (VkResult )vkBindImageMemory2KHR(_device, _bindInfoCount, _pBindInfos);
}
extern "C" void __c__vkGetDescriptorSetLayoutSupportKHR(VkDevice_T * _device, const VkDescriptorSetLayoutCreateInfo * _pCreateInfo, VkDescriptorSetLayoutSupport * _pSupport) {
    vkGetDescriptorSetLayoutSupportKHR(_device, _pCreateInfo, _pSupport);
}
extern "C" void __c__vkCmdDrawIndirectCountKHR(VkCommandBuffer_T * _commandBuffer, VkBuffer_T * _buffer, uint64_t _offset, VkBuffer_T * _countBuffer, uint64_t _countBufferOffset, uint32_t _maxDrawCount, uint32_t _stride) {
    vkCmdDrawIndirectCountKHR(_commandBuffer, _buffer, _offset, _countBuffer, _countBufferOffset, _maxDrawCount, _stride);
}
extern "C" void __c__vkCmdDrawIndexedIndirectCountKHR(VkCommandBuffer_T * _commandBuffer, VkBuffer_T * _buffer, uint64_t _offset, VkBuffer_T * _countBuffer, uint64_t _countBufferOffset, uint32_t _maxDrawCount, uint32_t _stride) {
    vkCmdDrawIndexedIndirectCountKHR(_commandBuffer, _buffer, _offset, _countBuffer, _countBufferOffset, _maxDrawCount, _stride);
}
extern "C" void __c__vkGetSemaphoreCounterValueKHR(VkResult *ret, VkDevice_T * _device, VkSemaphore_T * _semaphore, uint64_t * _pValue) {
    *ret = (VkResult )vkGetSemaphoreCounterValueKHR(_device, _semaphore, _pValue);
}
extern "C" void __c__vkWaitSemaphoresKHR(VkResult *ret, VkDevice_T * _device, const VkSemaphoreWaitInfo * _pWaitInfo, uint64_t _timeout) {
    *ret = (VkResult )vkWaitSemaphoresKHR(_device, _pWaitInfo, _timeout);
}
extern "C" void __c__vkSignalSemaphoreKHR(VkResult *ret, VkDevice_T * _device, const VkSemaphoreSignalInfo * _pSignalInfo) {
    *ret = (VkResult )vkSignalSemaphoreKHR(_device, _pSignalInfo);
}
extern "C" void __c__vkGetBufferDeviceAddressKHR(VkDeviceAddress *ret, VkDevice_T * _device, const VkBufferDeviceAddressInfo * _pInfo) {
    *ret = (uint64_t )vkGetBufferDeviceAddressKHR(_device, _pInfo);
}
extern "C" void __c__vkGetBufferOpaqueCaptureAddressKHR(uint64_t *ret, VkDevice_T * _device, const VkBufferDeviceAddressInfo * _pInfo) {
    *ret = (uint64_t )vkGetBufferOpaqueCaptureAddressKHR(_device, _pInfo);
}
extern "C" void __c__vkGetDeviceMemoryOpaqueCaptureAddressKHR(uint64_t *ret, VkDevice_T * _device, const VkDeviceMemoryOpaqueCaptureAddressInfo * _pInfo) {
    *ret = (uint64_t )vkGetDeviceMemoryOpaqueCaptureAddressKHR(_device, _pInfo);
}
extern "C" void __c__vkGetPipelineExecutablePropertiesKHR(VkResult *ret, VkDevice_T * _device, const VkPipelineInfoKHR * _pPipelineInfo, uint32_t * _pExecutableCount, VkPipelineExecutablePropertiesKHR * _pProperties) {
    *ret = (VkResult )vkGetPipelineExecutablePropertiesKHR(_device, _pPipelineInfo, _pExecutableCount, _pProperties);
}
extern "C" void __c__vkGetPipelineExecutableStatisticsKHR(VkResult *ret, VkDevice_T * _device, const VkPipelineExecutableInfoKHR * _pExecutableInfo, uint32_t * _pStatisticCount, VkPipelineExecutableStatisticKHR * _pStatistics) {
    *ret = (VkResult )vkGetPipelineExecutableStatisticsKHR(_device, _pExecutableInfo, _pStatisticCount, _pStatistics);
}
extern "C" void __c__vkGetPipelineExecutableInternalRepresentationsKHR(VkResult *ret, VkDevice_T * _device, const VkPipelineExecutableInfoKHR * _pExecutableInfo, uint32_t * _pInternalRepresentationCount, VkPipelineExecutableInternalRepresentationKHR * _pInternalRepresentations) {
    *ret = (VkResult )vkGetPipelineExecutableInternalRepresentationsKHR(_device, _pExecutableInfo, _pInternalRepresentationCount, _pInternalRepresentations);
}
extern "C" void __c__vkCreateDebugReportCallbackEXT(VkResult *ret, VkInstance_T * _instance, const VkDebugReportCallbackCreateInfoEXT * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkDebugReportCallbackEXT * _pCallback) {
    *ret = (VkResult )vkCreateDebugReportCallbackEXT(_instance, _pCreateInfo, _pAllocator, _pCallback);
}
extern "C" void __c__vkDestroyDebugReportCallbackEXT(VkInstance_T * _instance, VkDebugReportCallbackEXT_T * _callback, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyDebugReportCallbackEXT(_instance, _callback, _pAllocator);
}
extern "C" void __c__vkDebugReportMessageEXT(VkInstance_T * _instance, uint32_t _flags, VkDebugReportObjectTypeEXT _objectType, uint64_t _object, uint64_t _location, int32_t _messageCode, char * _pLayerPrefix, char * _pMessage) {
    vkDebugReportMessageEXT(_instance, _flags, _objectType, _object, _location, _messageCode, _pLayerPrefix, _pMessage);
}
extern "C" void __c__vkDebugMarkerSetObjectTagEXT(VkResult *ret, VkDevice_T * _device, const VkDebugMarkerObjectTagInfoEXT * _pTagInfo) {
    *ret = (VkResult )vkDebugMarkerSetObjectTagEXT(_device, _pTagInfo);
}
extern "C" void __c__vkDebugMarkerSetObjectNameEXT(VkResult *ret, VkDevice_T * _device, const VkDebugMarkerObjectNameInfoEXT * _pNameInfo) {
    *ret = (VkResult )vkDebugMarkerSetObjectNameEXT(_device, _pNameInfo);
}
extern "C" void __c__vkCmdDebugMarkerBeginEXT(VkCommandBuffer_T * _commandBuffer, const VkDebugMarkerMarkerInfoEXT * _pMarkerInfo) {
    vkCmdDebugMarkerBeginEXT(_commandBuffer, _pMarkerInfo);
}
extern "C" void __c__vkCmdDebugMarkerEndEXT(VkCommandBuffer_T * _commandBuffer) {
    vkCmdDebugMarkerEndEXT(_commandBuffer);
}
extern "C" void __c__vkCmdDebugMarkerInsertEXT(VkCommandBuffer_T * _commandBuffer, const VkDebugMarkerMarkerInfoEXT * _pMarkerInfo) {
    vkCmdDebugMarkerInsertEXT(_commandBuffer, _pMarkerInfo);
}
extern "C" void __c__vkCmdBindTransformFeedbackBuffersEXT(VkCommandBuffer_T * _commandBuffer, uint32_t _firstBinding, uint32_t _bindingCount, const VkBuffer * _pBuffers, const VkDeviceSize * _pOffsets, const VkDeviceSize * _pSizes) {
    vkCmdBindTransformFeedbackBuffersEXT(_commandBuffer, _firstBinding, _bindingCount, _pBuffers, _pOffsets, _pSizes);
}
extern "C" void __c__vkCmdBeginTransformFeedbackEXT(VkCommandBuffer_T * _commandBuffer, uint32_t _firstCounterBuffer, uint32_t _counterBufferCount, const VkBuffer * _pCounterBuffers, const VkDeviceSize * _pCounterBufferOffsets) {
    vkCmdBeginTransformFeedbackEXT(_commandBuffer, _firstCounterBuffer, _counterBufferCount, _pCounterBuffers, _pCounterBufferOffsets);
}
extern "C" void __c__vkCmdEndTransformFeedbackEXT(VkCommandBuffer_T * _commandBuffer, uint32_t _firstCounterBuffer, uint32_t _counterBufferCount, const VkBuffer * _pCounterBuffers, const VkDeviceSize * _pCounterBufferOffsets) {
    vkCmdEndTransformFeedbackEXT(_commandBuffer, _firstCounterBuffer, _counterBufferCount, _pCounterBuffers, _pCounterBufferOffsets);
}
extern "C" void __c__vkCmdBeginQueryIndexedEXT(VkCommandBuffer_T * _commandBuffer, VkQueryPool_T * _queryPool, uint32_t _query, uint32_t _flags, uint32_t _index) {
    vkCmdBeginQueryIndexedEXT(_commandBuffer, _queryPool, _query, _flags, _index);
}
extern "C" void __c__vkCmdEndQueryIndexedEXT(VkCommandBuffer_T * _commandBuffer, VkQueryPool_T * _queryPool, uint32_t _query, uint32_t _index) {
    vkCmdEndQueryIndexedEXT(_commandBuffer, _queryPool, _query, _index);
}
extern "C" void __c__vkCmdDrawIndirectByteCountEXT(VkCommandBuffer_T * _commandBuffer, uint32_t _instanceCount, uint32_t _firstInstance, VkBuffer_T * _counterBuffer, uint64_t _counterBufferOffset, uint32_t _counterOffset, uint32_t _vertexStride) {
    vkCmdDrawIndirectByteCountEXT(_commandBuffer, _instanceCount, _firstInstance, _counterBuffer, _counterBufferOffset, _counterOffset, _vertexStride);
}
extern "C" void __c__vkGetImageViewHandleNVX(uint32_t *ret, VkDevice_T * _device, const VkImageViewHandleInfoNVX * _pInfo) {
    *ret = (uint32_t )vkGetImageViewHandleNVX(_device, _pInfo);
}
extern "C" void __c__vkGetImageViewAddressNVX(VkResult *ret, VkDevice_T * _device, VkImageView_T * _imageView, VkImageViewAddressPropertiesNVX * _pProperties) {
    *ret = (VkResult )vkGetImageViewAddressNVX(_device, _imageView, _pProperties);
}
extern "C" void __c__vkCmdDrawIndirectCountAMD(VkCommandBuffer_T * _commandBuffer, VkBuffer_T * _buffer, uint64_t _offset, VkBuffer_T * _countBuffer, uint64_t _countBufferOffset, uint32_t _maxDrawCount, uint32_t _stride) {
    vkCmdDrawIndirectCountAMD(_commandBuffer, _buffer, _offset, _countBuffer, _countBufferOffset, _maxDrawCount, _stride);
}
extern "C" void __c__vkCmdDrawIndexedIndirectCountAMD(VkCommandBuffer_T * _commandBuffer, VkBuffer_T * _buffer, uint64_t _offset, VkBuffer_T * _countBuffer, uint64_t _countBufferOffset, uint32_t _maxDrawCount, uint32_t _stride) {
    vkCmdDrawIndexedIndirectCountAMD(_commandBuffer, _buffer, _offset, _countBuffer, _countBufferOffset, _maxDrawCount, _stride);
}
extern "C" void __c__vkGetShaderInfoAMD(VkResult *ret, VkDevice_T * _device, VkPipeline_T * _pipeline, VkShaderStageFlagBits _shaderStage, VkShaderInfoTypeAMD _infoType, size_t * _pInfoSize, void * _pInfo) {
    *ret = (VkResult )vkGetShaderInfoAMD(_device, _pipeline, _shaderStage, _infoType, _pInfoSize, _pInfo);
}
extern "C" void __c__vkGetPhysicalDeviceExternalImageFormatPropertiesNV(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, VkFormat _format, VkImageType _type, VkImageTiling _tiling, uint32_t _usage, uint32_t _flags, uint32_t _externalHandleType, VkExternalImageFormatPropertiesNV * _pExternalImageFormatProperties) {
    *ret = (VkResult )vkGetPhysicalDeviceExternalImageFormatPropertiesNV(_physicalDevice, _format, _type, _tiling, _usage, _flags, _externalHandleType, _pExternalImageFormatProperties);
}
extern "C" void __c__vkCmdBeginConditionalRenderingEXT(VkCommandBuffer_T * _commandBuffer, const VkConditionalRenderingBeginInfoEXT * _pConditionalRenderingBegin) {
    vkCmdBeginConditionalRenderingEXT(_commandBuffer, _pConditionalRenderingBegin);
}
extern "C" void __c__vkCmdEndConditionalRenderingEXT(VkCommandBuffer_T * _commandBuffer) {
    vkCmdEndConditionalRenderingEXT(_commandBuffer);
}
extern "C" void __c__vkCmdSetViewportWScalingNV(VkCommandBuffer_T * _commandBuffer, uint32_t _firstViewport, uint32_t _viewportCount, const VkViewportWScalingNV * _pViewportWScalings) {
    vkCmdSetViewportWScalingNV(_commandBuffer, _firstViewport, _viewportCount, _pViewportWScalings);
}
extern "C" void __c__vkReleaseDisplayEXT(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, VkDisplayKHR_T * _display) {
    *ret = (VkResult )vkReleaseDisplayEXT(_physicalDevice, _display);
}
extern "C" void __c__vkGetPhysicalDeviceSurfaceCapabilities2EXT(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, VkSurfaceKHR_T * _surface, VkSurfaceCapabilities2EXT * _pSurfaceCapabilities) {
    *ret = (VkResult )vkGetPhysicalDeviceSurfaceCapabilities2EXT(_physicalDevice, _surface, _pSurfaceCapabilities);
}
extern "C" void __c__vkDisplayPowerControlEXT(VkResult *ret, VkDevice_T * _device, VkDisplayKHR_T * _display, const VkDisplayPowerInfoEXT * _pDisplayPowerInfo) {
    *ret = (VkResult )vkDisplayPowerControlEXT(_device, _display, _pDisplayPowerInfo);
}
extern "C" void __c__vkRegisterDeviceEventEXT(VkResult *ret, VkDevice_T * _device, const VkDeviceEventInfoEXT * _pDeviceEventInfo, const VkAllocationCallbacks * _pAllocator, VkFence * _pFence) {
    *ret = (VkResult )vkRegisterDeviceEventEXT(_device, _pDeviceEventInfo, _pAllocator, _pFence);
}
extern "C" void __c__vkRegisterDisplayEventEXT(VkResult *ret, VkDevice_T * _device, VkDisplayKHR_T * _display, const VkDisplayEventInfoEXT * _pDisplayEventInfo, const VkAllocationCallbacks * _pAllocator, VkFence * _pFence) {
    *ret = (VkResult )vkRegisterDisplayEventEXT(_device, _display, _pDisplayEventInfo, _pAllocator, _pFence);
}
extern "C" void __c__vkGetSwapchainCounterEXT(VkResult *ret, VkDevice_T * _device, VkSwapchainKHR_T * _swapchain, VkSurfaceCounterFlagBitsEXT _counter, uint64_t * _pCounterValue) {
    *ret = (VkResult )vkGetSwapchainCounterEXT(_device, _swapchain, _counter, _pCounterValue);
}
extern "C" void __c__vkGetRefreshCycleDurationGOOGLE(VkResult *ret, VkDevice_T * _device, VkSwapchainKHR_T * _swapchain, VkRefreshCycleDurationGOOGLE * _pDisplayTimingProperties) {
    *ret = (VkResult )vkGetRefreshCycleDurationGOOGLE(_device, _swapchain, _pDisplayTimingProperties);
}
extern "C" void __c__vkGetPastPresentationTimingGOOGLE(VkResult *ret, VkDevice_T * _device, VkSwapchainKHR_T * _swapchain, uint32_t * _pPresentationTimingCount, VkPastPresentationTimingGOOGLE * _pPresentationTimings) {
    *ret = (VkResult )vkGetPastPresentationTimingGOOGLE(_device, _swapchain, _pPresentationTimingCount, _pPresentationTimings);
}
extern "C" void __c__vkCmdSetDiscardRectangleEXT(VkCommandBuffer_T * _commandBuffer, uint32_t _firstDiscardRectangle, uint32_t _discardRectangleCount, const VkRect2D * _pDiscardRectangles) {
    vkCmdSetDiscardRectangleEXT(_commandBuffer, _firstDiscardRectangle, _discardRectangleCount, _pDiscardRectangles);
}
extern "C" void __c__vkSetHdrMetadataEXT(VkDevice_T * _device, uint32_t _swapchainCount, const VkSwapchainKHR * _pSwapchains, const VkHdrMetadataEXT * _pMetadata) {
    vkSetHdrMetadataEXT(_device, _swapchainCount, _pSwapchains, _pMetadata);
}
extern "C" void __c__vkSetDebugUtilsObjectNameEXT(VkResult *ret, VkDevice_T * _device, const VkDebugUtilsObjectNameInfoEXT * _pNameInfo) {
    *ret = (VkResult )vkSetDebugUtilsObjectNameEXT(_device, _pNameInfo);
}
extern "C" void __c__vkSetDebugUtilsObjectTagEXT(VkResult *ret, VkDevice_T * _device, const VkDebugUtilsObjectTagInfoEXT * _pTagInfo) {
    *ret = (VkResult )vkSetDebugUtilsObjectTagEXT(_device, _pTagInfo);
}
extern "C" void __c__vkQueueBeginDebugUtilsLabelEXT(VkQueue_T * _queue, const VkDebugUtilsLabelEXT * _pLabelInfo) {
    vkQueueBeginDebugUtilsLabelEXT(_queue, _pLabelInfo);
}
extern "C" void __c__vkQueueEndDebugUtilsLabelEXT(VkQueue_T * _queue) {
    vkQueueEndDebugUtilsLabelEXT(_queue);
}
extern "C" void __c__vkQueueInsertDebugUtilsLabelEXT(VkQueue_T * _queue, const VkDebugUtilsLabelEXT * _pLabelInfo) {
    vkQueueInsertDebugUtilsLabelEXT(_queue, _pLabelInfo);
}
extern "C" void __c__vkCmdBeginDebugUtilsLabelEXT(VkCommandBuffer_T * _commandBuffer, const VkDebugUtilsLabelEXT * _pLabelInfo) {
    vkCmdBeginDebugUtilsLabelEXT(_commandBuffer, _pLabelInfo);
}
extern "C" void __c__vkCmdEndDebugUtilsLabelEXT(VkCommandBuffer_T * _commandBuffer) {
    vkCmdEndDebugUtilsLabelEXT(_commandBuffer);
}
extern "C" void __c__vkCmdInsertDebugUtilsLabelEXT(VkCommandBuffer_T * _commandBuffer, const VkDebugUtilsLabelEXT * _pLabelInfo) {
    vkCmdInsertDebugUtilsLabelEXT(_commandBuffer, _pLabelInfo);
}
extern "C" void __c__vkCreateDebugUtilsMessengerEXT(VkResult *ret, VkInstance_T * _instance, const VkDebugUtilsMessengerCreateInfoEXT * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkDebugUtilsMessengerEXT * _pMessenger) {
    *ret = (VkResult )vkCreateDebugUtilsMessengerEXT(_instance, _pCreateInfo, _pAllocator, _pMessenger);
}
extern "C" void __c__vkDestroyDebugUtilsMessengerEXT(VkInstance_T * _instance, VkDebugUtilsMessengerEXT_T * _messenger, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyDebugUtilsMessengerEXT(_instance, _messenger, _pAllocator);
}
extern "C" void __c__vkSubmitDebugUtilsMessageEXT(VkInstance_T * _instance, VkDebugUtilsMessageSeverityFlagBitsEXT _messageSeverity, uint32_t _messageTypes, const VkDebugUtilsMessengerCallbackDataEXT * _pCallbackData) {
    vkSubmitDebugUtilsMessageEXT(_instance, _messageSeverity, _messageTypes, _pCallbackData);
}
extern "C" void __c__vkCmdSetSampleLocationsEXT(VkCommandBuffer_T * _commandBuffer, const VkSampleLocationsInfoEXT * _pSampleLocationsInfo) {
    vkCmdSetSampleLocationsEXT(_commandBuffer, _pSampleLocationsInfo);
}
extern "C" void __c__vkGetPhysicalDeviceMultisamplePropertiesEXT(VkPhysicalDevice_T * _physicalDevice, VkSampleCountFlagBits _samples, VkMultisamplePropertiesEXT * _pMultisampleProperties) {
    vkGetPhysicalDeviceMultisamplePropertiesEXT(_physicalDevice, _samples, _pMultisampleProperties);
}
extern "C" void __c__vkGetImageDrmFormatModifierPropertiesEXT(VkResult *ret, VkDevice_T * _device, VkImage_T * _image, VkImageDrmFormatModifierPropertiesEXT * _pProperties) {
    *ret = (VkResult )vkGetImageDrmFormatModifierPropertiesEXT(_device, _image, _pProperties);
}
extern "C" void __c__vkCreateValidationCacheEXT(VkResult *ret, VkDevice_T * _device, const VkValidationCacheCreateInfoEXT * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkValidationCacheEXT * _pValidationCache) {
    *ret = (VkResult )vkCreateValidationCacheEXT(_device, _pCreateInfo, _pAllocator, _pValidationCache);
}
extern "C" void __c__vkDestroyValidationCacheEXT(VkDevice_T * _device, VkValidationCacheEXT_T * _validationCache, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyValidationCacheEXT(_device, _validationCache, _pAllocator);
}
extern "C" void __c__vkMergeValidationCachesEXT(VkResult *ret, VkDevice_T * _device, VkValidationCacheEXT_T * _dstCache, uint32_t _srcCacheCount, const VkValidationCacheEXT * _pSrcCaches) {
    *ret = (VkResult )vkMergeValidationCachesEXT(_device, _dstCache, _srcCacheCount, _pSrcCaches);
}
extern "C" void __c__vkGetValidationCacheDataEXT(VkResult *ret, VkDevice_T * _device, VkValidationCacheEXT_T * _validationCache, size_t * _pDataSize, void * _pData) {
    *ret = (VkResult )vkGetValidationCacheDataEXT(_device, _validationCache, _pDataSize, _pData);
}
extern "C" void __c__vkCmdBindShadingRateImageNV(VkCommandBuffer_T * _commandBuffer, VkImageView_T * _imageView, VkImageLayout _imageLayout) {
    vkCmdBindShadingRateImageNV(_commandBuffer, _imageView, _imageLayout);
}
extern "C" void __c__vkCmdSetViewportShadingRatePaletteNV(VkCommandBuffer_T * _commandBuffer, uint32_t _firstViewport, uint32_t _viewportCount, const VkShadingRatePaletteNV * _pShadingRatePalettes) {
    vkCmdSetViewportShadingRatePaletteNV(_commandBuffer, _firstViewport, _viewportCount, _pShadingRatePalettes);
}
extern "C" void __c__vkCmdSetCoarseSampleOrderNV(VkCommandBuffer_T * _commandBuffer, VkCoarseSampleOrderTypeNV _sampleOrderType, uint32_t _customSampleOrderCount, const VkCoarseSampleOrderCustomNV * _pCustomSampleOrders) {
    vkCmdSetCoarseSampleOrderNV(_commandBuffer, _sampleOrderType, _customSampleOrderCount, _pCustomSampleOrders);
}
extern "C" void __c__vkCreateAccelerationStructureNV(VkResult *ret, VkDevice_T * _device, const VkAccelerationStructureCreateInfoNV * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkAccelerationStructureNV * _pAccelerationStructure) {
    *ret = (VkResult )vkCreateAccelerationStructureNV(_device, _pCreateInfo, _pAllocator, _pAccelerationStructure);
}
extern "C" void __c__vkDestroyAccelerationStructureKHR(VkDevice_T * _device, VkAccelerationStructureKHR_T * _accelerationStructure, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyAccelerationStructureKHR(_device, _accelerationStructure, _pAllocator);
}
extern "C" void __c__vkDestroyAccelerationStructureNV(VkDevice_T * _device, VkAccelerationStructureKHR_T * _accelerationStructure, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyAccelerationStructureNV(_device, _accelerationStructure, _pAllocator);
}
extern "C" void __c__vkGetAccelerationStructureMemoryRequirementsNV(VkDevice_T * _device, const VkAccelerationStructureMemoryRequirementsInfoNV * _pInfo, VkMemoryRequirements2KHR * _pMemoryRequirements) {
    vkGetAccelerationStructureMemoryRequirementsNV(_device, _pInfo, _pMemoryRequirements);
}
extern "C" void __c__vkBindAccelerationStructureMemoryKHR(VkResult *ret, VkDevice_T * _device, uint32_t _bindInfoCount, const VkBindAccelerationStructureMemoryInfoKHR * _pBindInfos) {
    *ret = (VkResult )vkBindAccelerationStructureMemoryKHR(_device, _bindInfoCount, _pBindInfos);
}
extern "C" void __c__vkBindAccelerationStructureMemoryNV(VkResult *ret, VkDevice_T * _device, uint32_t _bindInfoCount, const VkBindAccelerationStructureMemoryInfoKHR * _pBindInfos) {
    *ret = (VkResult )vkBindAccelerationStructureMemoryNV(_device, _bindInfoCount, _pBindInfos);
}
extern "C" void __c__vkCmdBuildAccelerationStructureNV(VkCommandBuffer_T * _commandBuffer, const VkAccelerationStructureInfoNV * _pInfo, VkBuffer_T * _instanceData, uint64_t _instanceOffset, uint32_t _update, VkAccelerationStructureKHR_T * _dst, VkAccelerationStructureKHR_T * _src, VkBuffer_T * _scratch, uint64_t _scratchOffset) {
    vkCmdBuildAccelerationStructureNV(_commandBuffer, _pInfo, _instanceData, _instanceOffset, _update, _dst, _src, _scratch, _scratchOffset);
}
extern "C" void __c__vkCmdCopyAccelerationStructureNV(VkCommandBuffer_T * _commandBuffer, VkAccelerationStructureKHR_T * _dst, VkAccelerationStructureKHR_T * _src, VkCopyAccelerationStructureModeKHR _mode) {
    vkCmdCopyAccelerationStructureNV(_commandBuffer, _dst, _src, _mode);
}
extern "C" void __c__vkCmdTraceRaysNV(VkCommandBuffer_T * _commandBuffer, VkBuffer_T * _raygenShaderBindingTableBuffer, uint64_t _raygenShaderBindingOffset, VkBuffer_T * _missShaderBindingTableBuffer, uint64_t _missShaderBindingOffset, uint64_t _missShaderBindingStride, VkBuffer_T * _hitShaderBindingTableBuffer, uint64_t _hitShaderBindingOffset, uint64_t _hitShaderBindingStride, VkBuffer_T * _callableShaderBindingTableBuffer, uint64_t _callableShaderBindingOffset, uint64_t _callableShaderBindingStride, uint32_t _width, uint32_t _height, uint32_t _depth) {
    vkCmdTraceRaysNV(_commandBuffer, _raygenShaderBindingTableBuffer, _raygenShaderBindingOffset, _missShaderBindingTableBuffer, _missShaderBindingOffset, _missShaderBindingStride, _hitShaderBindingTableBuffer, _hitShaderBindingOffset, _hitShaderBindingStride, _callableShaderBindingTableBuffer, _callableShaderBindingOffset, _callableShaderBindingStride, _width, _height, _depth);
}
extern "C" void __c__vkCreateRayTracingPipelinesNV(VkResult *ret, VkDevice_T * _device, VkPipelineCache_T * _pipelineCache, uint32_t _createInfoCount, const VkRayTracingPipelineCreateInfoNV * _pCreateInfos, const VkAllocationCallbacks * _pAllocator, VkPipeline * _pPipelines) {
    *ret = (VkResult )vkCreateRayTracingPipelinesNV(_device, _pipelineCache, _createInfoCount, _pCreateInfos, _pAllocator, _pPipelines);
}
extern "C" void __c__vkGetRayTracingShaderGroupHandlesKHR(VkResult *ret, VkDevice_T * _device, VkPipeline_T * _pipeline, uint32_t _firstGroup, uint32_t _groupCount, uint64_t _dataSize, void * _pData) {
    *ret = (VkResult )vkGetRayTracingShaderGroupHandlesKHR(_device, _pipeline, _firstGroup, _groupCount, _dataSize, _pData);
}
extern "C" void __c__vkGetRayTracingShaderGroupHandlesNV(VkResult *ret, VkDevice_T * _device, VkPipeline_T * _pipeline, uint32_t _firstGroup, uint32_t _groupCount, uint64_t _dataSize, void * _pData) {
    *ret = (VkResult )vkGetRayTracingShaderGroupHandlesNV(_device, _pipeline, _firstGroup, _groupCount, _dataSize, _pData);
}
extern "C" void __c__vkGetAccelerationStructureHandleNV(VkResult *ret, VkDevice_T * _device, VkAccelerationStructureKHR_T * _accelerationStructure, uint64_t _dataSize, void * _pData) {
    *ret = (VkResult )vkGetAccelerationStructureHandleNV(_device, _accelerationStructure, _dataSize, _pData);
}
extern "C" void __c__vkCmdWriteAccelerationStructuresPropertiesKHR(VkCommandBuffer_T * _commandBuffer, uint32_t _accelerationStructureCount, const VkAccelerationStructureKHR * _pAccelerationStructures, VkQueryType _queryType, VkQueryPool_T * _queryPool, uint32_t _firstQuery) {
    vkCmdWriteAccelerationStructuresPropertiesKHR(_commandBuffer, _accelerationStructureCount, _pAccelerationStructures, _queryType, _queryPool, _firstQuery);
}
extern "C" void __c__vkCmdWriteAccelerationStructuresPropertiesNV(VkCommandBuffer_T * _commandBuffer, uint32_t _accelerationStructureCount, const VkAccelerationStructureKHR * _pAccelerationStructures, VkQueryType _queryType, VkQueryPool_T * _queryPool, uint32_t _firstQuery) {
    vkCmdWriteAccelerationStructuresPropertiesNV(_commandBuffer, _accelerationStructureCount, _pAccelerationStructures, _queryType, _queryPool, _firstQuery);
}
extern "C" void __c__vkCompileDeferredNV(VkResult *ret, VkDevice_T * _device, VkPipeline_T * _pipeline, uint32_t _shader) {
    *ret = (VkResult )vkCompileDeferredNV(_device, _pipeline, _shader);
}
extern "C" void __c__vkGetMemoryHostPointerPropertiesEXT(VkResult *ret, VkDevice_T * _device, VkExternalMemoryHandleTypeFlagBits _handleType, void * _pHostPointer, VkMemoryHostPointerPropertiesEXT * _pMemoryHostPointerProperties) {
    *ret = (VkResult )vkGetMemoryHostPointerPropertiesEXT(_device, _handleType, _pHostPointer, _pMemoryHostPointerProperties);
}
extern "C" void __c__vkCmdWriteBufferMarkerAMD(VkCommandBuffer_T * _commandBuffer, VkPipelineStageFlagBits _pipelineStage, VkBuffer_T * _dstBuffer, uint64_t _dstOffset, uint32_t _marker) {
    vkCmdWriteBufferMarkerAMD(_commandBuffer, _pipelineStage, _dstBuffer, _dstOffset, _marker);
}
extern "C" void __c__vkGetPhysicalDeviceCalibrateableTimeDomainsEXT(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, uint32_t * _pTimeDomainCount, VkTimeDomainEXT * _pTimeDomains) {
    *ret = (VkResult )vkGetPhysicalDeviceCalibrateableTimeDomainsEXT(_physicalDevice, _pTimeDomainCount, _pTimeDomains);
}
extern "C" void __c__vkGetCalibratedTimestampsEXT(VkResult *ret, VkDevice_T * _device, uint32_t _timestampCount, const VkCalibratedTimestampInfoEXT * _pTimestampInfos, uint64_t * _pTimestamps, uint64_t * _pMaxDeviation) {
    *ret = (VkResult )vkGetCalibratedTimestampsEXT(_device, _timestampCount, _pTimestampInfos, _pTimestamps, _pMaxDeviation);
}
extern "C" void __c__vkCmdDrawMeshTasksNV(VkCommandBuffer_T * _commandBuffer, uint32_t _taskCount, uint32_t _firstTask) {
    vkCmdDrawMeshTasksNV(_commandBuffer, _taskCount, _firstTask);
}
extern "C" void __c__vkCmdDrawMeshTasksIndirectNV(VkCommandBuffer_T * _commandBuffer, VkBuffer_T * _buffer, uint64_t _offset, uint32_t _drawCount, uint32_t _stride) {
    vkCmdDrawMeshTasksIndirectNV(_commandBuffer, _buffer, _offset, _drawCount, _stride);
}
extern "C" void __c__vkCmdDrawMeshTasksIndirectCountNV(VkCommandBuffer_T * _commandBuffer, VkBuffer_T * _buffer, uint64_t _offset, VkBuffer_T * _countBuffer, uint64_t _countBufferOffset, uint32_t _maxDrawCount, uint32_t _stride) {
    vkCmdDrawMeshTasksIndirectCountNV(_commandBuffer, _buffer, _offset, _countBuffer, _countBufferOffset, _maxDrawCount, _stride);
}
extern "C" void __c__vkCmdSetExclusiveScissorNV(VkCommandBuffer_T * _commandBuffer, uint32_t _firstExclusiveScissor, uint32_t _exclusiveScissorCount, const VkRect2D * _pExclusiveScissors) {
    vkCmdSetExclusiveScissorNV(_commandBuffer, _firstExclusiveScissor, _exclusiveScissorCount, _pExclusiveScissors);
}
extern "C" void __c__vkCmdSetCheckpointNV(VkCommandBuffer_T * _commandBuffer, void * _pCheckpointMarker) {
    vkCmdSetCheckpointNV(_commandBuffer, _pCheckpointMarker);
}
extern "C" void __c__vkGetQueueCheckpointDataNV(VkQueue_T * _queue, uint32_t * _pCheckpointDataCount, VkCheckpointDataNV * _pCheckpointData) {
    vkGetQueueCheckpointDataNV(_queue, _pCheckpointDataCount, _pCheckpointData);
}
extern "C" void __c__vkInitializePerformanceApiINTEL(VkResult *ret, VkDevice_T * _device, const VkInitializePerformanceApiInfoINTEL * _pInitializeInfo) {
    *ret = (VkResult )vkInitializePerformanceApiINTEL(_device, _pInitializeInfo);
}
extern "C" void __c__vkUninitializePerformanceApiINTEL(VkDevice_T * _device) {
    vkUninitializePerformanceApiINTEL(_device);
}
extern "C" void __c__vkCmdSetPerformanceMarkerINTEL(VkResult *ret, VkCommandBuffer_T * _commandBuffer, const VkPerformanceMarkerInfoINTEL * _pMarkerInfo) {
    *ret = (VkResult )vkCmdSetPerformanceMarkerINTEL(_commandBuffer, _pMarkerInfo);
}
extern "C" void __c__vkCmdSetPerformanceStreamMarkerINTEL(VkResult *ret, VkCommandBuffer_T * _commandBuffer, const VkPerformanceStreamMarkerInfoINTEL * _pMarkerInfo) {
    *ret = (VkResult )vkCmdSetPerformanceStreamMarkerINTEL(_commandBuffer, _pMarkerInfo);
}
extern "C" void __c__vkCmdSetPerformanceOverrideINTEL(VkResult *ret, VkCommandBuffer_T * _commandBuffer, const VkPerformanceOverrideInfoINTEL * _pOverrideInfo) {
    *ret = (VkResult )vkCmdSetPerformanceOverrideINTEL(_commandBuffer, _pOverrideInfo);
}
extern "C" void __c__vkAcquirePerformanceConfigurationINTEL(VkResult *ret, VkDevice_T * _device, const VkPerformanceConfigurationAcquireInfoINTEL * _pAcquireInfo, VkPerformanceConfigurationINTEL * _pConfiguration) {
    *ret = (VkResult )vkAcquirePerformanceConfigurationINTEL(_device, _pAcquireInfo, _pConfiguration);
}
extern "C" void __c__vkReleasePerformanceConfigurationINTEL(VkResult *ret, VkDevice_T * _device, VkPerformanceConfigurationINTEL_T * _configuration) {
    *ret = (VkResult )vkReleasePerformanceConfigurationINTEL(_device, _configuration);
}
extern "C" void __c__vkQueueSetPerformanceConfigurationINTEL(VkResult *ret, VkQueue_T * _queue, VkPerformanceConfigurationINTEL_T * _configuration) {
    *ret = (VkResult )vkQueueSetPerformanceConfigurationINTEL(_queue, _configuration);
}
extern "C" void __c__vkGetPerformanceParameterINTEL(VkResult *ret, VkDevice_T * _device, VkPerformanceParameterTypeINTEL _parameter, VkPerformanceValueINTEL * _pValue) {
    *ret = (VkResult )vkGetPerformanceParameterINTEL(_device, _parameter, _pValue);
}
extern "C" void __c__vkSetLocalDimmingAMD(VkDevice_T * _device, VkSwapchainKHR_T * _swapChain, uint32_t _localDimmingEnable) {
    vkSetLocalDimmingAMD(_device, _swapChain, _localDimmingEnable);
}
extern "C" void __c__vkGetBufferDeviceAddressEXT(VkDeviceAddress *ret, VkDevice_T * _device, const VkBufferDeviceAddressInfo * _pInfo) {
    *ret = (uint64_t )vkGetBufferDeviceAddressEXT(_device, _pInfo);
}
extern "C" void __c__vkGetPhysicalDeviceToolPropertiesEXT(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, uint32_t * _pToolCount, VkPhysicalDeviceToolPropertiesEXT * _pToolProperties) {
    *ret = (VkResult )vkGetPhysicalDeviceToolPropertiesEXT(_physicalDevice, _pToolCount, _pToolProperties);
}
extern "C" void __c__vkGetPhysicalDeviceCooperativeMatrixPropertiesNV(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, uint32_t * _pPropertyCount, VkCooperativeMatrixPropertiesNV * _pProperties) {
    *ret = (VkResult )vkGetPhysicalDeviceCooperativeMatrixPropertiesNV(_physicalDevice, _pPropertyCount, _pProperties);
}
extern "C" void __c__vkGetPhysicalDeviceSupportedFramebufferMixedSamplesCombinationsNV(VkResult *ret, VkPhysicalDevice_T * _physicalDevice, uint32_t * _pCombinationCount, VkFramebufferMixedSamplesCombinationNV * _pCombinations) {
    *ret = (VkResult )vkGetPhysicalDeviceSupportedFramebufferMixedSamplesCombinationsNV(_physicalDevice, _pCombinationCount, _pCombinations);
}
extern "C" void __c__vkCreateHeadlessSurfaceEXT(VkResult *ret, VkInstance_T * _instance, const VkHeadlessSurfaceCreateInfoEXT * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkSurfaceKHR * _pSurface) {
    *ret = (VkResult )vkCreateHeadlessSurfaceEXT(_instance, _pCreateInfo, _pAllocator, _pSurface);
}
extern "C" void __c__vkCmdSetLineStippleEXT(VkCommandBuffer_T * _commandBuffer, uint32_t _lineStippleFactor, uint16_t _lineStipplePattern) {
    vkCmdSetLineStippleEXT(_commandBuffer, _lineStippleFactor, _lineStipplePattern);
}
extern "C" void __c__vkResetQueryPoolEXT(VkDevice_T * _device, VkQueryPool_T * _queryPool, uint32_t _firstQuery, uint32_t _queryCount) {
    vkResetQueryPoolEXT(_device, _queryPool, _firstQuery, _queryCount);
}
extern "C" void __c__vkCmdSetCullModeEXT(VkCommandBuffer_T * _commandBuffer, uint32_t _cullMode) {
    vkCmdSetCullModeEXT(_commandBuffer, _cullMode);
}
extern "C" void __c__vkCmdSetFrontFaceEXT(VkCommandBuffer_T * _commandBuffer, VkFrontFace _frontFace) {
    vkCmdSetFrontFaceEXT(_commandBuffer, _frontFace);
}
extern "C" void __c__vkCmdSetPrimitiveTopologyEXT(VkCommandBuffer_T * _commandBuffer, VkPrimitiveTopology _primitiveTopology) {
    vkCmdSetPrimitiveTopologyEXT(_commandBuffer, _primitiveTopology);
}
extern "C" void __c__vkCmdSetViewportWithCountEXT(VkCommandBuffer_T * _commandBuffer, uint32_t _viewportCount, const VkViewport * _pViewports) {
    vkCmdSetViewportWithCountEXT(_commandBuffer, _viewportCount, _pViewports);
}
extern "C" void __c__vkCmdSetScissorWithCountEXT(VkCommandBuffer_T * _commandBuffer, uint32_t _scissorCount, const VkRect2D * _pScissors) {
    vkCmdSetScissorWithCountEXT(_commandBuffer, _scissorCount, _pScissors);
}
extern "C" void __c__vkCmdBindVertexBuffers2EXT(VkCommandBuffer_T * _commandBuffer, uint32_t _firstBinding, uint32_t _bindingCount, const VkBuffer * _pBuffers, const VkDeviceSize * _pOffsets, const VkDeviceSize * _pSizes, const VkDeviceSize * _pStrides) {
    vkCmdBindVertexBuffers2EXT(_commandBuffer, _firstBinding, _bindingCount, _pBuffers, _pOffsets, _pSizes, _pStrides);
}
extern "C" void __c__vkCmdSetDepthTestEnableEXT(VkCommandBuffer_T * _commandBuffer, uint32_t _depthTestEnable) {
    vkCmdSetDepthTestEnableEXT(_commandBuffer, _depthTestEnable);
}
extern "C" void __c__vkCmdSetDepthWriteEnableEXT(VkCommandBuffer_T * _commandBuffer, uint32_t _depthWriteEnable) {
    vkCmdSetDepthWriteEnableEXT(_commandBuffer, _depthWriteEnable);
}
extern "C" void __c__vkCmdSetDepthCompareOpEXT(VkCommandBuffer_T * _commandBuffer, VkCompareOp _depthCompareOp) {
    vkCmdSetDepthCompareOpEXT(_commandBuffer, _depthCompareOp);
}
extern "C" void __c__vkCmdSetDepthBoundsTestEnableEXT(VkCommandBuffer_T * _commandBuffer, uint32_t _depthBoundsTestEnable) {
    vkCmdSetDepthBoundsTestEnableEXT(_commandBuffer, _depthBoundsTestEnable);
}
extern "C" void __c__vkCmdSetStencilTestEnableEXT(VkCommandBuffer_T * _commandBuffer, uint32_t _stencilTestEnable) {
    vkCmdSetStencilTestEnableEXT(_commandBuffer, _stencilTestEnable);
}
extern "C" void __c__vkCmdSetStencilOpEXT(VkCommandBuffer_T * _commandBuffer, uint32_t _faceMask, VkStencilOp _failOp, VkStencilOp _passOp, VkStencilOp _depthFailOp, VkCompareOp _compareOp) {
    vkCmdSetStencilOpEXT(_commandBuffer, _faceMask, _failOp, _passOp, _depthFailOp, _compareOp);
}
extern "C" void __c__vkGetGeneratedCommandsMemoryRequirementsNV(VkDevice_T * _device, const VkGeneratedCommandsMemoryRequirementsInfoNV * _pInfo, VkMemoryRequirements2 * _pMemoryRequirements) {
    vkGetGeneratedCommandsMemoryRequirementsNV(_device, _pInfo, _pMemoryRequirements);
}
extern "C" void __c__vkCmdPreprocessGeneratedCommandsNV(VkCommandBuffer_T * _commandBuffer, const VkGeneratedCommandsInfoNV * _pGeneratedCommandsInfo) {
    vkCmdPreprocessGeneratedCommandsNV(_commandBuffer, _pGeneratedCommandsInfo);
}
extern "C" void __c__vkCmdExecuteGeneratedCommandsNV(VkCommandBuffer_T * _commandBuffer, uint32_t _isPreprocessed, const VkGeneratedCommandsInfoNV * _pGeneratedCommandsInfo) {
    vkCmdExecuteGeneratedCommandsNV(_commandBuffer, _isPreprocessed, _pGeneratedCommandsInfo);
}
extern "C" void __c__vkCmdBindPipelineShaderGroupNV(VkCommandBuffer_T * _commandBuffer, VkPipelineBindPoint _pipelineBindPoint, VkPipeline_T * _pipeline, uint32_t _groupIndex) {
    vkCmdBindPipelineShaderGroupNV(_commandBuffer, _pipelineBindPoint, _pipeline, _groupIndex);
}
extern "C" void __c__vkCreateIndirectCommandsLayoutNV(VkResult *ret, VkDevice_T * _device, const VkIndirectCommandsLayoutCreateInfoNV * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkIndirectCommandsLayoutNV * _pIndirectCommandsLayout) {
    *ret = (VkResult )vkCreateIndirectCommandsLayoutNV(_device, _pCreateInfo, _pAllocator, _pIndirectCommandsLayout);
}
extern "C" void __c__vkDestroyIndirectCommandsLayoutNV(VkDevice_T * _device, VkIndirectCommandsLayoutNV_T * _indirectCommandsLayout, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyIndirectCommandsLayoutNV(_device, _indirectCommandsLayout, _pAllocator);
}
extern "C" void __c__vkCreatePrivateDataSlotEXT(VkResult *ret, VkDevice_T * _device, const VkPrivateDataSlotCreateInfoEXT * _pCreateInfo, const VkAllocationCallbacks * _pAllocator, VkPrivateDataSlotEXT * _pPrivateDataSlot) {
    *ret = (VkResult )vkCreatePrivateDataSlotEXT(_device, _pCreateInfo, _pAllocator, _pPrivateDataSlot);
}
extern "C" void __c__vkDestroyPrivateDataSlotEXT(VkDevice_T * _device, VkPrivateDataSlotEXT_T * _privateDataSlot, const VkAllocationCallbacks * _pAllocator) {
    vkDestroyPrivateDataSlotEXT(_device, _privateDataSlot, _pAllocator);
}
extern "C" void __c__vkSetPrivateDataEXT(VkResult *ret, VkDevice_T * _device, VkObjectType _objectType, uint64_t _objectHandle, VkPrivateDataSlotEXT_T * _privateDataSlot, uint64_t _data) {
    *ret = (VkResult )vkSetPrivateDataEXT(_device, _objectType, _objectHandle, _privateDataSlot, _data);
}
extern "C" void __c__vkGetPrivateDataEXT(VkDevice_T * _device, VkObjectType _objectType, uint64_t _objectHandle, VkPrivateDataSlotEXT_T * _privateDataSlot, uint64_t * _pData) {
    vkGetPrivateDataEXT(_device, _objectType, _objectHandle, _privateDataSlot, _pData);
}
