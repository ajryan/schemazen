namespace model {
	internal enum State {
		Searching,
		InOneLineComment,
		InMultiLineComment,
		InBrackets,
		InQuotes
	}
}