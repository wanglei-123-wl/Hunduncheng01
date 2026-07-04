package main

import (
	"encoding/json"
	"log"
	"net/http"
)

type LoginRequest struct {
	Username string `json:"username"`
	Password string `json:"password"`
}

type LoginResponse struct {
	Success bool   `json:"success"`
	Message string `json:"message"`
	Token   string `json:"token"`
}

func main() {
	http.HandleFunc("/api/login", loginHandler)
	log.Println("login server listening on http://127.0.0.1:8080")
	log.Fatal(http.ListenAndServe(":8080", nil))
}

func loginHandler(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "application/json")
	w.Header().Set("Access-Control-Allow-Origin", "*")
	w.Header().Set("Access-Control-Allow-Headers", "Content-Type")
	w.Header().Set("Access-Control-Allow-Methods", "POST, OPTIONS")

	if r.Method == http.MethodOptions {
		w.WriteHeader(http.StatusNoContent)
		return
	}

	if r.Method != http.MethodPost {
		w.WriteHeader(http.StatusMethodNotAllowed)
		writeJSON(w, LoginResponse{Success: false, Message: "only POST is supported"})
		return
	}

	var req LoginRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		writeJSON(w, LoginResponse{Success: false, Message: "bad request json"})
		return
	}

	if req.Username == "test" && req.Password == "123456" {
		writeJSON(w, LoginResponse{Success: true, Message: "login ok", Token: "test-token-123"})
		return
	}

	writeJSON(w, LoginResponse{Success: false, Message: "wrong username or password"})
}

func writeJSON(w http.ResponseWriter, response LoginResponse) {
	if err := json.NewEncoder(w).Encode(response); err != nil {
		log.Println("write response failed:", err)
	}
}
