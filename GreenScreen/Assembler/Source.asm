
.data
	dqwColorArray db 16 dup (?) ;Array which will fill xmm register with bytest from color to remove.
.code
processPictureAssembler PROC
		;Move data aqquired from C# function call to general purpouse registers.
		MOV r9, rcx ;Photo pixels in bytes.
		MOV r10, rdx ;Removable color.
		MOV r11, r8 ;Amount of bytes (in array stored in r9 register).

		XOR rcx, rcx ;Clear counter.
		MOV rax, OFFSET dqwColorArray ;Move array address ro RAX 

;Fill dqwColorArray with color to remove.
;Sequence: A R G B
xmmFilerLoop:
		;Alpha is hardcoded to 255 and in not taken from GUI
		XOR r12,r12
		NOT r12
		MOV [rax], r12b
		
		;R value is read from GUI input and saved to array
		ADD rax,TYPE dqwColorArray 
		MOV r12b, [r10]
		MOV [rax], r12b
		
		;G value is read from GUI input and saved to array
		ADD rax,TYPE dqwColorArray 
		MOV r12b, [r10+1]
		MOV [rax], r12b

		;B value is read from GUI input and saved to array
		ADD rax,TYPE dqwColorArray 
		MOV r12b, [r10+2]
		MOV [rax], r12b

		;Move further in dqwColorArray and increment counter by 4.
		ADD rax,TYPE dqwColorArray 
		ADD rcx, 4

		;Chcek if whole array was filled.
		CMP rcx, 16
		JNE xmmFilerLoop
		
		;Use array to fill xmm0 register
		MOV rax, OFFSET dqwColorArray
		MOVDQU xmm0, [rax] ;Move double quad word unalligned

;Procces picture data 

		;Picture size smaller ten 4 px (16 bytes)
		CMP r11, 16
		JB lessThen16 ;If smaller jump
		
		MOV rax, r9 ;Push pixelArray (in bytes) to RAX
sseLoop:		
		MOVDQU xmm1, [rax] ;Move 16 bytes (4 pixels) from PixelArray to xmm
		PCMPEQD xmm0, xmm1 ;Check if quadwords (pixels) are equals if so change in xmm0 pixel bits to 1
		PCMPEQD xmm2, xmm2 ;Set all xmm2 bits to 1
		PXOR xmm0,xmm2 ;PXOR + previous instruction makes bitwise negation of xmm0 bits 
		PAND xmm1, xmm0 ;Clear appropriate bits in xmm1
		MOVDQU [rax], xmm1 ;Save changed pixels back to array

		ADD rax, 16 ;Mov to next bytes in pixelArray
		
		SUB r11, 16 ;Reduce unprocessed bytes amount by 16
		CMP r11, 0 ;Check if bytes amount is enough to fill xmm register.
		JLE restPixelLoop
		
		
		;Refil xmm0
		MOV r13, OFFSET dqwColorArray 
		MOVDQU xmm0, [r13]
		JMP sseLoop
;Responsible for rest of pixels (less than 16 left)
restPixelLoop:
		;Else proces leftovers
		add r11, 16

	
lessThen16:
		;If all processed exit
		CMP r11, 0 
		JZ exit	

		;Check A
		mov r12b, 255
		mov r13b, [rax]
		cmp [rax],r12b
		jne bypassPixel
		;Check R
		mov r12b, [r10]
		mov r13b, [rax + 1]
		cmp [rax + 1], r12b
		jne bypassPixel
		;Check G
		mov r12b, [r10 + 1]
		mov r13b, [rax + 2]
		cmp [rax + 2], r12b
		jne bypassPixel
		;Check B
		mov r12b, [r10 + 2]
		mov r13b, [rax + 3]
		cmp [rax + 3], r12b
		jne bypassPixel
		
		;Clear pixel
		xor r14,r14
		mov [rax],r14b
		mov [rax+1],r14b
		mov [rax+2],r14b
		mov [rax+3],r14b
		mov r13b,[rax]
		mov r13b,[rax+1]
		mov r13b,[rax+2]
		mov r13b,[rax+3]
bypassPixel:
		;If some byte of pixel is not equal to removable color than go to next pixel.
		sub r11, 4
		
		;CMP r11, 0 
		;JZ exit	
		add rax, 4
		jmp lessThen16
exit:
mov rax,0
ret

processPictureAssembler ENDP

end
