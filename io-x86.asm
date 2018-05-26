.model flat, c

option casemap:none

include D:\masm32\include\user32.inc
include D:\masm32\include\kernel32.inc
include D:\masm32\include\windows.inc
;include C:\Irvine\SmallWin.inc

_input_buffer_size = 256

.data 

; handles
stdOutHandle dword 0
stdErrHandle dword 0
stdInputHandle dword 0

bytesWritten dword ?
bytesRead dword ?
singleCharBuffer byte ?
strPointer dword 0
strLength dword 0
inputBuffer byte _input_buffer_size dup(0)

STD_OUTPUT_HANDLE = -11

.code

GetStdOutHandle proc

	; check if we already have a handle
	cmp dword ptr [stdOutHandle], 0
	jne l_end			; we already have a handle

	; get std out handle
	push STD_OUTPUT_HANDLE
	call GetStdHandle	; returns handle in eax

	; save handle
	mov dword ptr [stdOutHandle], eax

l_end:
	; return the handle
	mov eax, dword ptr [stdOutHandle]

	ret
GetStdOutHandle endp

_GetStdInputHandle proc
	; check if we already have a handle
	cmp dword ptr [stdInputHandle], 0
	jne l_end			; we already have a handle

	; get std out handle
	push STD_INPUT_HANDLE
	call GetStdHandle	; returns handle in eax

	; save handle
	mov dword ptr [stdInputHandle], eax

l_end:
	; return the handle
	mov eax, dword ptr [stdInputHandle]

	ret
_GetStdInputHandle endp

; =============================
; Returns the length of a zero terminated string
;
; ecx: pointer to string
; =============================
_GetStringLength proc
	; eax: stores str len
	mov ecx, dword ptr [esp + 4]
	mov eax, 0

l_begin:
	cmp byte ptr [ecx], 0
	je l_end
	inc eax
	inc ecx
	jmp l_begin

	
l_end:

	ret
_GetStringLength endp

; =============================
; Prints a single character to standard out
;
; cl: character to be printed
; =============================
_PrintChar proc
	; mov param to singleCharBuffer
	mov cl, byte ptr [esp + 4]
	mov byte ptr [singleCharBuffer], cl
	
	; check if we already have a handle
	cmp dword ptr [stdOutHandle], 0
	jne l_printChar

	; get std out handle
	push STD_OUTPUT_HANDLE
	call GetStdHandle

	mov dword ptr [stdOutHandle], eax	; return value
	cmp dword ptr [stdOutHandle], 0		; check if we got a handle now
	mov eax, 1
	je l_end

l_printChar:

	; print character
	push 0
	lea eax, dword ptr [bytesWritten]
	push eax
	push 1
	lea eax, dword ptr [singleCharBuffer]
	push eax
	push dword ptr [stdOutHandle]
	call WriteFile

	mov eax, 0

l_end:
	ret
_PrintChar endp

; =============================
; Print a specific number of characters to standard out
;
; ecx: pointer to characters
; edx: amount of characters to be printed
; =============================
_PrintChars proc
	mov ecx, dword ptr [esp + 4]
	mov edx, dword ptr [esp + 8]

	; mov pointer to strPointer
	mov dword ptr [strPointer], ecx

	; mov string length to strLength
	mov dword ptr [strLength], edx

	; check if we already have a handle
	cmp dword ptr [stdOutHandle], 0
	jne l_printChar

	; get std out handle
	push STD_OUTPUT_HANDLE
	call GetStdHandle

	mov dword ptr [stdOutHandle], eax	; return value
	cmp dword ptr [stdOutHandle], 0		; check if we got a handle now
	mov eax, 1
	je l_end

l_printChar:

	; print character
	push 0
	lea eax, dword ptr [bytesWritten]
	push eax
	push dword ptr [strLength]
	push dword ptr [strPointer]
	push dword ptr [stdOutHandle]
	call WriteFile

	mov eax, 0

l_end:

	mov eax, dword ptr [strLength]
	ret
_PrintChars endp

; =============================
; Prints a string to standard out
;
; ecx: pointer to string
; =============================
PrintString proc
	push ebp
	mov ebp, esp

	; calculate length of string
	push dword ptr [ebp + 8]
	call _GetStringLength
	add esp, 4

	; print the characters
	push eax 
	push dword ptr [ebp + 8]
	call _PrintChars
	add esp, 8

	pop ebp
	ret
PrintString endp

; =============================
; Prints a string to standard out and adds a new line
;
; ecx: pointer to string
; =============================
Println proc
	push ebp
	mov ebp, esp

	; print the string
	push dword ptr [ebp + 8]
	call PrintString
	add esp, 4

	; print new line
	sub esp, 4
	mov dword ptr [esp], 13
	call _PrintChar
	mov dword ptr [esp], 10
	call _PrintChar
	add esp, 4

	pop ebp
	ret
Println endp

; =============================
; Opens a message box with a supplied text, title and style
;
; ebp + 8:  ptr to message
; ebp + 12: ptr to title
; ebp + 16: type
; =============================
_MessageBox proc
	push ebp
	mov ebp, esp

	push dword ptr [ebp + 16]	; push type
	push dword ptr [ebp + 12]	; push title
	push dword ptr [ebp + 8]	; push message
	push 0						; push handle
	call MessageBoxA

	pop ebp
	ret
_MessageBox endp


; =============================
; Reads in a single character and returns it, waiting for input
;
; =============================
_ReadChar proc
	call _GetStdInputHandle
	cmp eax, 0
	je l_noHandle

	; we have a handle
	push 0			; lpReserved
	lea eax, dword ptr [bytesRead]
	push eax		; lpNumberOfCharsRead
	push 3			; nNumberOfCharsToRead  - 3 because of char and end line chars (\r\n)
	lea eax, dword ptr [inputBuffer]
	push eax		; lpBuffer
	push dword ptr [stdInputHandle]
	call ReadConsoleA

	; return character in al
	mov al, byte ptr [inputBuffer]
	jmp l_end

l_noHandle:
	xor eax, eax	; return 0 if no handle

l_end:
	ret
_ReadChar endp

end
