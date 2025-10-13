import { useState } from "react";
import { Send } from "lucide-react";
import axios from "axios";
import ReactMarkdown from 'react-markdown'; // Importe ReactMarkdown
import rehypeRaw from 'rehype-raw';     // Importe rehypeRaw para HTML cru, se necessário

export default function ChatbotPage() {
    const [messages, setMessages] = useState([
        { sender: "bot", text: "Olá! Como seu TechMentor, estou pronto para ajudar com qualquer questão de TI." }
    ]);
    const [input, setInput] = useState("");
    const [isLoading, setIsLoading] = useState(false);

    const handleSend = async () => {
        if (!input.trim()) return;

        const userInput = input;

        setMessages(prevMessages => [...prevMessages, { sender: "user", text: userInput }]);

        setInput("");

        setIsLoading(true);

        try {
            const response = await axios.post(
                'http://localhost:5224/api/chat/ask', 
                { question: userInput }
            );

            const aiResponseText = response.data.answer;

            setMessages(prevMessages => [...prevMessages, { sender: "bot", text: aiResponseText }]);

        } catch (error) {
            console.error("Error connecting to the AI service:", error);
            setMessages(prevMessages => [...prevMessages, { sender: "bot", text: "Desculpe, houve um erro ao conectar ao serviço de IA. Por favor, tente novamente." }]);
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="flex flex-col h-screen bg-gradient-to-b from-black via-gray-900 to-purple-900 text-gray-100 font-sans">
            {/* Header */}
            <header className="bg-black/40 backdrop-blur-md border-b border-gray-700 p-4 text-center text-2xl font-semibold text-purple-400 shadow-md">
                Shinka Chat
            </header>

            {/* Chat Area */}
            <div className="flex-1 overflow-y-auto p-6 space-y-4 scrollbar-thin scrollbar-thumb-purple-700 scrollbar-track-gray-800">
                {messages.map((msg, index) => (
                    <div
                        key={index}
                        className={`flex ${msg.sender === "user" ? "justify-end" : "justify-start"}`}
                    >
                        <div
                            className={`max-w-xs md:max-w-md px-4 py-2 rounded-2xl text-sm md:text-base shadow-lg ${msg.sender === "user"
                                ? "bg-purple-700 text-white rounded-br-none"
                                : "bg-gray-800 text-gray-100 rounded-bl-none"
                                }`}
                        >
                            {/* AQUI É ONDE USAMOS O ReactMarkdown */}
                            {msg.sender === "user" 
                                ? msg.text 
                                : <ReactMarkdown rehypePlugins={[rehypeRaw]}>{msg.text}</ReactMarkdown>
                            }
                        </div>
                    </div>
                ))}
                {isLoading && (
                    <div className="flex justify-start">
                        <div className="max-w-xs md:max-w-md px-4 py-2 rounded-2xl text-sm md:text-base shadow-lg bg-gray-800 text-gray-100 rounded-bl-none">
                            Pensando...
                        </div>
                    </div>
                )}
            </div>

            {/* Input Area */}
            <div className="flex items-center gap-2 p-4 bg-black/60 border-t border-gray-800">
                <input
                    type="text"
                    value={input}
                    onChange={(e) => setInput(e.target.value)}
                    onKeyDown={(e) => e.key === "Enter" && handleSend()}
                    placeholder="Type a message..."
                    className="flex-1 bg-gray-900 text-gray-100 placeholder-gray-500 border border-gray-700 rounded-xl px-4 py-2 focus:outline-none focus:ring-2 focus:ring-purple-600"
                    disabled={isLoading}
                />
                <button
                    onClick={handleSend}
                    className="p-3 bg-purple-700 hover:bg-purple-800 rounded-xl transition-all duration-300 shadow-md"
                >
                    <Send size={20} />
                </button>
            </div>
        </div>
    );
}