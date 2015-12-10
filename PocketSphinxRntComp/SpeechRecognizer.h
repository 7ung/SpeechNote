#pragma once

/**
* Created by Toine de Boer, Enbyin (NL)
* 
* Intended as kick-start using PocketSphinx on Windows mobile platforms
* Methods are listed in order of usage
*/

namespace PocketSphinxRntComp
{
	public delegate void ResultFoundHandler(Platform::String^ result);

	public ref class SpeechRecognizer sealed
    {
    public:
		SpeechRecognizer();

		// STEP 1: Initialize and Load searches

		/* Step 1.1: Quy định từ điển phiên âm */
		Platform::String^ Initialize(Platform::String^ hmmFilePath, Platform::String^ dictFilePath);

		/* Step 1.2: Quy định từ khoá tìm kiếm, hệ thống sẽ focus tìm khi mình phát âm keyphrase */
		Platform::String^ AddKeyphraseSearch(Platform::String^ name, Platform::String^ keyphrase);

		/* Step 1.3: Quy định bộ ngữ pháp */
		Platform::String^ AddGrammarSearch(Platform::String^ name, Platform::String^ filePath);

		/* Step 1.4: Unknow */
		Platform::String^ AddNgramSearch(Platform::String^ name, Platform::String^ filePath);
		
		// STEP 2: Set search
		Platform::String^ SetSearch(Platform::String^ name);		

		// STEP 3: Start processing
		Platform::String^ StartProcessing(void);
		Platform::String^ StopProcessing(void);
		Platform::String^ RestartProcessing(void);
		Platform::Boolean IsProcessing(void);
		
		// STEP 4: Register incomming audio
		int SpeechRecognizer::RegisterAudioBytes(const Platform::Array<uint8>^ audioBytes);	
				
		// STEP 5: Wait for results
		
		event ResultFoundHandler^ resultFound;
		event ResultFoundHandler^ resultFinalizedBySilence; // Sự kiện khi im lặng -> nói xong
		/*
		ResultFoundHandler(Platform::String^ result);
		hai event kiểu ResultFoundHandler truyền vào string. 
		resultFound cho result là kết quả tính toán tạm thời..
		resultFinalizedBySilence cho result là kết quả cuối cùng
		*/


		Platform::String^ CleanPocketSphinx(void);

		// Test method (to use a raw recorded audio file)
		Platform::String^ TestPocketSphinx(void);

	private:
		void SpeechRecognizer::OnResultFound(Platform::String^ result);
		void SpeechRecognizer::OnResultFinalizedBySilence(Platform::String^ finalResult);
		Platform::Boolean IsReadyForProcessing(Platform::String^& message);
    };
}